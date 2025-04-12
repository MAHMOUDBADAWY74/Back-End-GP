﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.BookService.Dtos
{
    public class BookDetailsDto
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }
        public string? Summary { get; set; }
        public string? Text { get; set; }
        public string? Cover { get; set; } // رابط URL للصورة
        public IFormFile? NewCover { get; set; } // الصورة الجديدة
    }
}
