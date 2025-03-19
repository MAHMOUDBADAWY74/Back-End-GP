using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Specification
{
    public class BookSpecification : BaseSpecification<BooksDatum>
    {
        public BookSpecification(int pageIndex, int pageSize)
            : base()
        {
            ApplyPagination((pageIndex - 1) * pageSize, pageSize);
        }
    }

}
