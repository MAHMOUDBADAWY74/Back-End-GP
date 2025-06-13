using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Service.BookService;
using OnlineLibrary.Service.BookService.Dtos;
using OnlineLibrary.Web.Hubs;
using OnlineLibrary.Web.Hubs.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : BaseController
    {
        private readonly IBookService _bookService;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly OnlineLibraryIdentityDbContext _dbContext;
        private readonly IWebHostEnvironment _env; 

        public BooksController(
            IBookService bookService,
            IHubContext<NotificationHub> notificationHub,
            UserManager<ApplicationUser> userManager,
            OnlineLibraryIdentityDbContext dbContext,
            IWebHostEnvironment env 
        )
        {
            _bookService = bookService;
            _notificationHub = notificationHub;
            _userManager = userManager;
            _dbContext = dbContext;
            _env = env; 
        }

        private string GetUserId() => _userManager.GetUserId(User);

        private async Task<(string Username, string ProfilePicture)> GetUserDetails(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var userProfile = await _dbContext.UserProfiles
                .FirstOrDefaultAsync(up => up.UserId == userId);

            string username = user != null ? $"{user.firstName} {user.LastName}" : "Unknown";
            string profilePicture = userProfile?.ProfilePhoto ?? "default_profile.jpg";

            return (username, profilePicture);
        }

        private string GetTimeAgo(DateTime createdAt)
        {
            var minutes = (int)(DateTime.UtcNow - createdAt).TotalMinutes;
            if (minutes < 60)
                return $"{minutes} min ago";
            var hours = (int)(DateTime.UtcNow - createdAt).TotalHours;
            if (hours < 24)
                return $"{hours} h ago";
            var days = (int)(DateTime.UtcNow - createdAt).TotalDays;
            return $"{days} d ago";
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            if (books == null || !books.Any())
                return NotFound("No books available.");
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBookById(long id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchBooks([FromQuery] string term)
        {
            var books = await _bookService.SearchBooksAsync(term);
            return Ok(books);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddBook([FromForm] AddBookDetailsDto addBookDetailsDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string? epubFilePath = null;
            if (addBookDetailsDto.EpubFile != null && addBookDetailsDto.EpubFile.Length > 0)
            {
                var downloadsFolder = Path.Combine(_env.WebRootPath, "downloads");
                if (!Directory.Exists(downloadsFolder))
                    Directory.CreateDirectory(downloadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{addBookDetailsDto.EpubFile.FileName}";
                var filePath = Path.Combine(downloadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await addBookDetailsDto.EpubFile.CopyToAsync(stream);
                }

                epubFilePath = $"/downloads/{uniqueFileName}";
            }

            await _bookService.AddBookAsync(addBookDetailsDto);

            var userId = GetUserId();
            var (username, profilePicture) = await GetUserDetails(userId);

            var notification = new NotificationDto
            {
                Id = 0,
                NotificationType = "BookAdded",
                Message = $"A new book '{addBookDetailsDto.Title}' has been added by {username}!",
                ActorUserId = userId,
                ActorUserName = username,
                ActorProfilePicture = profilePicture,
                RelatedEntityId = null,
                CreatedAt = DateTime.UtcNow,
                TimeAgo = GetTimeAgo(DateTime.UtcNow)
            };

            await _notificationHub.Clients.GroupExcept("AllUsers", userId)
                .SendAsync("ReceiveNotification", notification);
            Console.WriteLine($"Sending notification to all users: {notification.Message}");

            return Ok(new { message = "Book added successfully.", epubPath = epubFilePath });
        }

        [HttpPut]
        public async Task<ActionResult> UpdateBook([FromForm] BookDetailsDto bookDetailsDto)
        {
            await _bookService.UpdateBookAsync(bookDetailsDto);
            return Ok("Book updated successfully.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteBook(long id)
        {
            await _bookService.DeleteBookAsync(id);
            return Ok();
        }

        [HttpDelete("{id}/cover")]
        public async Task<IActionResult> RemoveBookCover(long id)
        {
            await _bookService.RemoveBookCoverAsync(id);
            return Ok("Cover removed successfully.");
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetBooksCount()
        {
            var count = await _dbContext.BooksData.AsNoTracking().CountAsync();
            return Ok(count);
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedBookDto>> GetBooksPaginated([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _bookService.GetAllBooksAsyncUsingPaginated(pageIndex, pageSize);
            return Ok(result);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            var categories = await _bookService.GetAllCategoriesAsync();
            if (categories == null || !categories.Any())
                return NotFound("No categories available.");
            return Ok(categories);
        }

        [HttpPost("wishlist/add")]
        [Authorize]
        public async Task<IActionResult> AddToWishlist([FromQuery] long bookId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var exists = await _dbContext.Wishlists.AnyAsync(w => w.UserId == userId && w.BookId == bookId);
            if (exists)
                return BadRequest("Book already in wishlist.");

            var wishlist = new Wishlist
            {
                UserId = userId,
                BookId = bookId,
                AddedOn = DateTime.UtcNow
            };
            _dbContext.Wishlists.Add(wishlist);
            await _dbContext.SaveChangesAsync();

            return Ok("Book added to wishlist.");
        }

        [HttpDelete("wishlist/remove")]
        [Authorize]
        public async Task<IActionResult> RemoveFromWishlist([FromQuery] long bookId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var wishlist = await _dbContext.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);
            if (wishlist == null)
                return NotFound("Book not found in wishlist.");

            _dbContext.Wishlists.Remove(wishlist);
            await _dbContext.SaveChangesAsync();

            return Ok("Book removed from wishlist.");
        }

        [HttpDelete("wishlist/clear")]
        [Authorize]
        public async Task<IActionResult> ClearWishlist()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var wishlists = await _dbContext.Wishlists.Where(w => w.UserId == userId).ToListAsync();
            if (!wishlists.Any())
                return NotFound("Wishlist is already empty.");

            _dbContext.Wishlists.RemoveRange(wishlists);
            await _dbContext.SaveChangesAsync();

            return Ok("Wishlist cleared.");
        }

        [HttpGet("wishlist/all")]
        [Authorize]
        public async Task<IActionResult> GetWishlistBooks()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var bookIds = await _dbContext.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => w.BookId)
                .ToListAsync();

            var books = await _dbContext.BooksData
                .Where(b => bookIds.Contains(b.Id))
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Category,
                    b.Author,
                    b.Summary,
                    b.Cover
                })
                .ToListAsync();

            return Ok(books);
        }
    }
}
