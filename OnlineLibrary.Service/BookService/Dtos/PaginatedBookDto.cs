using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.BookService.Dtos
{
    public class PaginatedBookDto
    {
        public IReadOnlyList<GetAllBookDetailsDto> Books { get; set; } 
        public int TotalCount { get; set; } 
        public int PageNumber { get; set; } 
        public int PageSize { get; set; } 

        public PaginatedBookDto()
        {
            Books = new List<GetAllBookDetailsDto>();
        }
    }
}
