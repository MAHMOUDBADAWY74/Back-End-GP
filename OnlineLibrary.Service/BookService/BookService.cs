using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specification;
using OnlineLibrary.Service.BookService.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.BookService
{
    public class BookService : IBookService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public BookService(IUnitOfWork unitOfWork, IMapper mapper) 
        {

            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddBookAsync(AddBookDetailsDto addBookDetailsDto)
        {
            var book = _mapper.Map<BooksDatum>(addBookDetailsDto); 
            await _unitOfWork.Repository<BooksDatum>().AddAsync(book);
            await _unitOfWork.CountAsync();
        }

        public async  Task DeleteBookAsync(long id)
        {
            var book = await _unitOfWork.Repository<BooksDatum>().GetByIdAsync((int)id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            _unitOfWork.Repository<BooksDatum>().Delete(book);
            await _unitOfWork.CountAsync();
        }

       

        public async Task<IReadOnlyList<GetAllBookDetailsDto>> GetAllBooksAsync()
        {


            var books = await _unitOfWork.Repository<BooksDatum>().GetAllAsync();

            return _mapper.Map<IReadOnlyList<GetAllBookDetailsDto>>(books);
        }



        public async  Task<BookDetailsDto> GetBookByIdAsync(long id)
        {
            var book = await _unitOfWork.Repository<BooksDatum>().GetByIdAsync((int)id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            return _mapper.Map<BookDetailsDto>(book);
        }

        public async Task UpdateBookAsync(BookDetailsDto BookDetailsDto)
        {
            var book = await _unitOfWork.Repository<BooksDatum>().GetByIdAsync((int)BookDetailsDto.Id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            _mapper.Map(BookDetailsDto, book); 
            _unitOfWork.Repository<BooksDatum>().Update(book);
            await _unitOfWork.CountAsync();
        }


        public async Task<PaginatedBookDto> GetAllBooksAsyncUsingPaginated(int pageIndex, int pageSize)
        {
            var spec = new BookSpecification(pageIndex, pageSize);

            // Get paginated books
            var books = await _unitOfWork.Repository<BooksDatum>().GetAllWithSpecAsync(spec);

            var totalCount = await _unitOfWork.Repository<BooksDatum>().CountWithSpecAsync(new BookSpecification(1, int.MaxValue));

            return new PaginatedBookDto
            {
                Books = _mapper.Map<IReadOnlyList<GetAllBookDetailsDto>>(books),
                TotalCount = totalCount,
                PageNumber = pageIndex,
                PageSize = pageSize
            };
        }




    }
}
