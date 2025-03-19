using OnlineLibrary.Service.BookService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.BookService
{
    public interface IBookService
    {

        Task<BookDetailsDto> GetBookByIdAsync(long id);
        Task<IReadOnlyList<GetAllBookDetailsDto>> GetAllBooksAsync();
        Task AddBookAsync(AddBookDetailsDto addBookDetailsDto);
        Task UpdateBookAsync(BookDetailsDto BookDetailsDto);
        Task DeleteBookAsync(long id);
        Task<PaginatedBookDto> GetAllBooksAsyncUsingPaginated(int pageIndex, int pageSize);
    }
}
