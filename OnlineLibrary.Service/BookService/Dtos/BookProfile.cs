using AutoMapper;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.BookService.Dtos
{
   public class BookProfile : Profile
    {
        public BookProfile()
        {
            CreateMap<string, CategoryDto>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => src));
            CreateMap<BooksDatum, AddBookDetailsDto>().ReverseMap();
            CreateMap<BooksDatum, BookDetailsDto>().ReverseMap();
            CreateMap<BooksDatum, GetAllBookDetailsDto>().ReverseMap();
            CreateMap<BooksDatum, PaginatedBookDto>().ReverseMap();

        }
    }
}
