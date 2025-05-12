using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OnlineLibrary.Service.BookService;
using OnlineLibrary.Service.BookService.Dtos;
using OnlineLibrary.Web.Hubs; 
using System.Threading.Tasks;

namespace OnlineLibrary.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : BaseController
    {
        private readonly IBookService _bookService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public BooksController(
            IBookService bookService,
            IHubContext<NotificationHub> notificationHub) 
        {
            _bookService = bookService;
            _notificationHub = notificationHub;
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddBook([FromForm] AddBookDetailsDto addBookDetailsDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _bookService.AddBookAsync(addBookDetailsDto);

            
            string message = $"A new book '{addBookDetailsDto.Title}' has been added!";
            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", message);
            Console.WriteLine($"Sending notification to all users: {message}");

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