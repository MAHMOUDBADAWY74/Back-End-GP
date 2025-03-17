using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
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

        public async Task<IReadOnlyList<BookDetailsDto>> GetAllBooksAsync()
        {


            var books = await _unitOfWork.Repository<BooksDatum>().GetAllAsync();
            
            return _mapper.Map<IReadOnlyList<BookDetailsDto>>(books);
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

       


        //public async Task<IReadOnlyList<BooksDatum>> GetAllBooksAsync()
        //{
        //    var books = await _unitOfWork.Repository<BooksDatum>().GetAllAsync();

        //    Console.WriteLine($"Total Books Found: {books.Count}"); 

        //    return books;
        //}

    }
}
