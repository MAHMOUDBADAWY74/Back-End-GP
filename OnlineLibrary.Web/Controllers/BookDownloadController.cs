using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Hosting;

namespace OnlineLibrary.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookDownloadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpClientFactory _httpClientFactory;

        public BookDownloadController(IWebHostEnvironment env, IHttpClientFactory httpClientFactory)
        {
            _env = env;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadBook(long id)
        {
            var downloadsDir = Path.Combine(_env.WebRootPath, "downloads");
            if (!Directory.Exists(downloadsDir))
                Directory.CreateDirectory(downloadsDir);

            var fileName = $"book_{id}.epub";
            var filePath = Path.Combine(downloadsDir, fileName);
            var relativePath = $"/downloads/{fileName}";
            var bookUrl = $"https://www.gutenberg.org/ebooks/{id}.epub3.images";

            if (System.IO.File.Exists(filePath))
            {
                return Ok(new
                {
                    path = relativePath,
                    message = "Book already downloaded"
                });
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
                );
                var response = await client.GetAsync(bookUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, new
                    {
                        error = "Failed to download book.",
                        details = $"Status: {response.StatusCode}, URL: {bookUrl}"
                    });
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                return Ok(new
                {
                    path = relativePath,
                    message = "Book downloaded successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to download book.",
                    details = ex.Message
                });
            }
        }
    }
}
