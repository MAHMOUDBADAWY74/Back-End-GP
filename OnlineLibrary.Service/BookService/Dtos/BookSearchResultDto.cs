using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.BookService.Dtos
{
    public class BookSearchResultDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Cover { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
    }

}
