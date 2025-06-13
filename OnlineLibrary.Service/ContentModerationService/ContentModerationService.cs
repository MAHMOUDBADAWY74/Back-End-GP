using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OnlineLibrary.Service.ContentModerationService
{
    public class ContentModerationService : IContentModerationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ContentModerationService> _logger;
        private readonly string _hfApiKey;
        private readonly string _hfArabicModelUrl;
        private readonly string _perspectiveApiKey;
        private readonly string _perspectiveEndpoint;

        private readonly List<string> _offensiveKeywords = new List<string>
        {
            // الكلمات المسيئة بالإنجليزية
            "idiot", "stupid", "moron", "loser", "jerk", "fool", "dumb", "douche", "clueless", "garbage",
            "trash", "pathetic", "worthless", "retard", "asshole", "bastard", "bitch", "damn", "shit",
            "crap", "hell", "suck", "shut up", "freak", "lame", "creep", "scum", "whore", "slut",
            // الكلمات المسيئة بالعربية (فصحى وعامية)
            "غبي", "أحمق", "تافه", "فاشل", "حقير", "سافل", "حيوان", "قمامة", "مهزوز", "وضيع",
            "كلب", "خنزير", "حمار", "جاهل", "مقرف", "قذر", "غتيت", "بليد", "معوق", "منحط",
            "نذل", "خسيس", "وقح", "قليل الأدب", "متخلف", "رذيل", "لئيم", "سخيف", "مغفل",
            "زبالة", "فاسد", "منحرف", "كذاب", "نصاب", "خائن", "مش عاجبني", "يا واطي",
            // كلمات عامية (مصرية)
            "رخم", "زفت", "عبيط", "غلس", "متنيل", "هطل", "بلطجي", "يا زبالة", "يا معفن",
            "يا نكسة", "يا فج", "يا دبش", "يا بهيمة", "يا جزمة",
            // كلمات عامية (خليجية)
            "ثور", "داشر", "سخيف", "يا قليل", "يا ناقص", "يا تافه", "يا جايف", "يا رذيل",
            "يا خايس", "يا بقرة",
            // عبارات مسيئة شائعة
            "انت غبي", "يا فاشل", "مش عايز أشوفك", "روح من هنا", "يا حقير", "يا تافه",
            "you're an idiot", "go to hell", "you're trash", "what a loser", "you're pathetic"
        };

        public ContentModerationService(IConfiguration configuration, ILogger<ContentModerationService> logger)
        {
            _logger = logger;
            _hfApiKey = configuration["HuggingFace:ApiKey"];
            _hfArabicModelUrl = configuration["HuggingFace:ArabicModelUrl"] ?? "https://api-inference.huggingface.co/models/aubmindlab/bert-base-arabic";
            _perspectiveApiKey = configuration["Perspective:ApiKey"];
            _perspectiveEndpoint = configuration["Perspective:Endpoint"] ?? "https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze";

            if (string.IsNullOrEmpty(_perspectiveApiKey))
            {
                _logger.LogError("Perspective API Key is missing in configuration.");
                throw new ArgumentNullException(nameof(_perspectiveApiKey), "Perspective API Key is required.");
            }

            _httpClient = new HttpClient();
            _logger.LogInformation("ContentModerationService initialized.");
        }

        public async Task<ContentModerationResult> ModerateTextAsync(string text)
        {
            _logger.LogInformation($"Moderating text: {text}");
            string lowerText = text.ToLowerInvariant();

            var offensivePatterns = new Dictionary<string, string>
            {
                { "idiot", @"\b[i1!][dcl][i1!][o0][t7]\b" },
                { "stupid", @"\b[s5][t7][uü][p1!][i1!][dcl]\b" },
                { "غبي", @"\bغ\s*ب\s*[يي]\b" },
                { "أحمق", @"\bأ\s*ح\s*م\s*ق\b" },
                { "shit", @"\b[s5][h#][i1!][t7]\b" },
                { "قمامة", @"\bق\s*م\s*ا\s*م\s*ة\b" },
                { "asshole", @"\b[a@][s5][s5][h#][o0][l1!][e3]\b" },
                { "عبيط", @"\bع\s*ب\s*ي\s*ط\b" },
                { "زفت", @"\bز\s*ف\s*ت\b" }
            };

            foreach (var pattern in offensivePatterns)
            {
                if (Regex.IsMatch(lowerText, pattern.Value, RegexOptions.IgnoreCase))
                {
                    _logger.LogWarning($"Offensive pattern detected: {pattern.Key}");
                    return new ContentModerationResult
                    {
                        IsAppropriate = false,
                        Category = "KeywordDetected",
                        ReasonMessage = $"Offensive word detected: {pattern.Key}"
                    };
                }
            }

            foreach (var keyword in _offensiveKeywords)
            {
                if (Regex.IsMatch(lowerText, $@"\b{keyword}\b", RegexOptions.IgnoreCase))
                {
                    _logger.LogWarning($"Offensive keyword detected: {keyword}");
                    return new ContentModerationResult
                    {
                        IsAppropriate = false,
                        Category = "KeywordDetected",
                        ReasonMessage = $"Offensive word detected: {keyword}"
                    };
                }
            }

            var arabicText = string.Join(" ", Regex.Matches(text, @"[\u0600-\u06FF]+").Cast<Match>().Select(m => m.Value));
            var englishText = string.Join(" ", Regex.Matches(text, @"[a-zA-Z]+").Cast<Match>().Select(m => m.Value));

            try
            {
                if (!string.IsNullOrEmpty(arabicText))
                {
                    var arabicResult = await AnalyzeWithHuggingFace(arabicText, _hfArabicModelUrl);
                    if (!arabicResult.IsAppropriate)
                    {
                        _logger.LogWarning($"Arabic text flagged: {arabicResult.ReasonMessage}");
                        return arabicResult;
                    }
                }

                if (!string.IsNullOrEmpty(englishText))
                {
                    var englishResult = await AnalyzeWithPerspective(englishText);
                    if (!englishResult.IsAppropriate)
                    {
                        _logger.LogWarning($"English text flagged: {englishResult.ReasonMessage}");
                        return englishResult;
                    }
                }

                _logger.LogInformation("Text deemed appropriate.");
                return new ContentModerationResult { IsAppropriate = true };
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError($"HTTP Request Error: {httpEx.Message}");
                return new ContentModerationResult
                {
                    IsAppropriate = false,
                    Category = "ServiceError",
                    ReasonMessage = "An error occurred in the content moderation service. Please try again later."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                return new ContentModerationResult { IsAppropriate = true };
            }
        }

        private async Task<ContentModerationResult> AnalyzeWithHuggingFace(string text, string modelUrl)
        {
            var requestBody = new { inputs = text };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, modelUrl)
            {
                Content = content
            };
            request.Headers.Add("Authorization", $"Bearer {_hfApiKey}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Hugging Face API response: {responseBody}");

            var hfResponse = JsonSerializer.Deserialize<List<List<Dictionary<string, JsonElement>>>>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (hfResponse == null || !hfResponse.Any() || !hfResponse[0].Any())
            {
                _logger.LogWarning("Invalid or empty response from Hugging Face API.");
                return new ContentModerationResult
                {
                    IsAppropriate = false,
                    Category = "ServiceError",
                    ReasonMessage = "Failed to analyze API response."
                };
            }

            var predictions = hfResponse[0];
            var okCategory = predictions.FirstOrDefault(p => p.ContainsKey("label") && p["label"].GetString() == "OK");

            if (okCategory != null && okCategory.ContainsKey("score") && okCategory["score"].GetDouble() > 0.6)
            {
                return new ContentModerationResult { IsAppropriate = true };
            }
            else
            {
                var inappropriateCategories = predictions
                    .Where(p => p.ContainsKey("label") && p["label"].GetString() != "OK" && p["score"].GetDouble() > 0.4)
                    .OrderByDescending(p => p.ContainsKey("score") ? p["score"].GetDouble() : 0)
                    .ToList();

                if (inappropriateCategories.Any())
                {
                    var topInappropriate = inappropriateCategories.First();
                    string label = topInappropriate["label"].GetString();
                    double score = topInappropriate["score"].GetDouble();
                    return new ContentModerationResult
                    {
                        IsAppropriate = false,
                        Category = label,
                        ReasonMessage = GetReasonMessage(label) + $" (Probability: {score:P2})"
                    };
                }
            }

            return new ContentModerationResult { IsAppropriate = true };
        }

        private async Task<ContentModerationResult> AnalyzeWithPerspective(string text)
        {
            var requestBody = new
            {
                comment = new { text = text },
                requestedAttributes = new
                {
                    TOXICITY = new { },
                    INSULT = new { }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var requestUrl = $"{_perspectiveEndpoint}?key={_perspectiveApiKey}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = content
            };
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogDebug($"Perspective API response: {responseBody}");

            var perspectiveResponse = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (perspectiveResponse == null || !perspectiveResponse.TryGetValue("attributeScores", out var attributeScores))
            {
                _logger.LogWarning("Invalid or empty response from Perspective API.");
                return new ContentModerationResult
                {
                    IsAppropriate = false,
                    Category = "ServiceError",
                    ReasonMessage = "Failed to analyze Perspective API response."
                };
            }

            double toxicityScore = 0;
            double insultScore = 0;

            if (attributeScores.TryGetProperty("TOXICITY", out var toxicity) &&
                toxicity.TryGetProperty("summaryScore", out var toxicitySummary) &&
                toxicitySummary.TryGetProperty("value", out var toxicityValue))
            {
                toxicityScore = toxicityValue.GetDouble();
            }

            if (attributeScores.TryGetProperty("INSULT", out var insult) &&
                insult.TryGetProperty("summaryScore", out var insultSummary) &&
                insultSummary.TryGetProperty("value", out var insultValue))
            {
                insultScore = insultValue.GetDouble();
            }

            if (toxicityScore > 0.7 || insultScore > 0.7)
            {
                string category = toxicityScore > insultScore ? "TOXICITY" : "INSULT";
                double score = Math.Max(toxicityScore, insultScore);
                return new ContentModerationResult
                {
                    IsAppropriate = false,
                    Category = category,
                    ReasonMessage = GetReasonMessage(category) + $" (Probability: {score:P2})"
                };
            }

            return new ContentModerationResult { IsAppropriate = true };
        }

        private string GetReasonMessage(string category)
        {
            return category switch
            {
                "S" => "Potential sexual content",
                "H" => "Potential hate content",
                "V" => "Potential violent content",
                "HR" => "Potential harassment content",
                "SH" => "Potential self-harm content",
                "S3" => "Potential child sexual content",
                "H2" => "Potential hate or threat content",
                "V2" => "Potential explicit violent content",
                "KeywordDetected" => "Offensive word detected",
                "TOXICITY" => "Potential toxic content",
                "INSULT" => "Potential insult content",
                _ => "Inappropriate content"
            };
        }
    }
}
