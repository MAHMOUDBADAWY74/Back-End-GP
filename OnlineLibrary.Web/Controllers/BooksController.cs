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

        public BooksController(
            IBookService bookService,
            IHubContext<NotificationHub> notificationHub,
            UserManager<ApplicationUser> userManager,
            OnlineLibraryIdentityDbContext dbContext)
        {
            _bookService = bookService;
            _notificationHub = notificationHub;
            _userManager = userManager;
            _dbContext = dbContext;
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

            await _bookService.AddBookAsync(addBookDetailsDto);

            var userId = GetUserId();
            var (username, profilePicture) = await GetUserDetails(userId);

            var notification = new NotificationDto
            {
                Id = 0, // لا يوجد Id حقيقي هنا، يمكن تجاهله أو تعيينه لاحقاً إذا لزم الأمر
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

            return Ok("Book added successfully.");
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
    }
}
