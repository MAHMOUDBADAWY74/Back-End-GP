using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.Service.BookService;
using OnlineLibrary.Service.BookService.Dtos;

namespace OnlineLibrary.Web.Controllers
{
    
    public class BooksController : BaseController
    {

        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }




        [HttpGet]
        //public async Task<IActionResult> GetAllBooks()
        //{
        //    var books = await _bookService.GetAllBooksAsync();
        //    return Ok(books);
        //}
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
        public async Task<ActionResult> AddBook(AddBookDetailsDto addBookDetailsDto)
        {
            await _bookService.AddBookAsync(addBookDetailsDto);
            return Ok();
        }



        [HttpPut]
        public async Task<ActionResult> UpdateBook(BookDetailsDto BookDetailsDto)
        {
            await _bookService.UpdateBookAsync( BookDetailsDto);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteBook(long id)
        {
            await _bookService.DeleteBookAsync(id);
            return Ok();
        }
        [HttpGet]
        public async Task<ActionResult<PaginatedBookDto>> GetBooksPaginated([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _bookService.GetAllBooksAsyncUsingPaginated(pageIndex, pageSize);
            return Ok(result);
        }
    }
}
