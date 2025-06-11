using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.ContentModerationService
{
    public interface IContentModerationService
    {
        Task<ContentModerationResult> ModerateTextAsync(string text);
    }

    public class ContentModerationResult
    {
        public bool IsAppropriate { get; set; }
        public string Category { get; set; }
        public string ReasonMessage { get; set; }
    }
}
