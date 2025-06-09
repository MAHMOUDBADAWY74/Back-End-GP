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
        Task AddBookAsync(AddBookDetailsDto addBookDetailsDto);
        Task DeleteBookAsync(long id);
        Task RemoveBookCoverAsync(long id);
        Task<IEnumerable<BookSearchResultDto>> SearchBooksAsync(string term);

        Task<IReadOnlyList<GetAllBookDetailsDto>> GetAllBooksAsync();
        Task<BookDetailsDto> GetBookByIdAsync(long id);
        Task UpdateBookAsync(BookDetailsDto bookDetailsDto);
        Task<PaginatedBookDto> GetAllBooksAsyncUsingPaginated(int pageIndex, int pageSize);
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    }
}