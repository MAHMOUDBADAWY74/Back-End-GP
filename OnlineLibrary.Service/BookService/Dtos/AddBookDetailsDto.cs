using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.BookService.Dtos
{
        public class AddBookDetailsDto
        {
            [Required(ErrorMessage = "Title is required.")]
            public string? Title { get; set; }

            [Required(ErrorMessage = "Author is required.")]
            public string? Author { get; set; }

            [Required(ErrorMessage = "Category is required.")]
            public string? Category { get; set; }

            [Required(ErrorMessage = "Summary is required.")]
            public string? Summary { get; set; }

            [Required(ErrorMessage = "EPUB file is required.")]
            public IFormFile? EpubFile { get; set; }

            [Required(ErrorMessage = "Cover image is required.")]
            public IFormFile? Cover { get; set; }
        }
    }



