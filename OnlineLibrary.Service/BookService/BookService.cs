using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specification;
using OnlineLibrary.Service.BookService.Dtos;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            string? coverUrl = null;

            if (addBookDetailsDto.Cover != null)
            {
                var sanitizedTitle = addBookDetailsDto.Title?.Replace(" ", "_").Replace(":", "").Replace("/", "") ?? "Unknown";
                var fileName = $"{sanitizedTitle}_{Guid.NewGuid()}{Path.GetExtension(addBookDetailsDto.Cover.FileName)}";
                var filePath = Path.Combine("wwwroot/images", fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await addBookDetailsDto.Cover.CopyToAsync(stream);
                    }
                    coverUrl = $"/images/{fileName}";
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("An error occurred while saving the cover image.", ex);
                }
            }

            var book = _mapper.Map<BooksDatum>(addBookDetailsDto);
            book.Cover = coverUrl;

            await _unitOfWork.Repository<BooksDatum>().AddAsync(book);
            await _unitOfWork.CountAsync();
        }

        public async Task DeleteBookAsync(long id)
        {
            var book = await _unitOfWork.Repository<BooksDatum>().GetByIdAsync((int)id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            if (!string.IsNullOrEmpty(book.Cover))
            {
                var filePath = Path.Combine("wwwroot", book.Cover.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _unitOfWork.Repository<BooksDatum>().Delete(book);
            await _unitOfWork.CountAsync();
        }

        public async Task RemoveBookCoverAsync(long id)
        {
            var book = await _unitOfWork.Repository<BooksDatum>().GetByIdAsync((int)id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            if (!string.IsNullOrEmpty(book.Cover))
            {
                var filePath = Path.Combine("wwwroot", book.Cover.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                book.Cover = null;
                _unitOfWork.Repository<BooksDatum>().Update(book);
                await _unitOfWork.CountAsync();
            }
        }

        public async Task<IReadOnlyList<GetAllBookDetailsDto>> GetAllBooksAsync()
        {
            var books = await _unitOfWork.Repository<BooksDatum>().GetAllAsync();
            return _mapper.Map<IReadOnlyList<GetAllBookDetailsDto>>(books);
        }

        public async Task<BookDetailsDto> GetBookByIdAsync(long id)
        {
            var book = await _unitOfWork.Repository<BooksDatum>().GetByIdAsync((int)id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            return _mapper.Map<BookDetailsDto>(book);
        }

        public async Task UpdateBookAsync(BookDetailsDto bookDetailsDto)
        {
            var book = await _unitOfWork.Repository<BooksDatum>().GetByIdAsync((int)bookDetailsDto.Id);
            if (book == null)
            {
                throw new KeyNotFoundException("Book not found.");
            }

            if (bookDetailsDto.NewCover != null)
            {
                if (!string.IsNullOrEmpty(book.Cover))
                {
                    var oldFilePath = Path.Combine("wwwroot", book.Cover.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                var sanitizedTitle = bookDetailsDto.Title?.Replace(" ", "_").Replace(":", "").Replace("/", "") ?? "Unknown";
                var fileName = $"{sanitizedTitle}_{Guid.NewGuid()}{Path.GetExtension(bookDetailsDto.NewCover.FileName)}";
                var filePath = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bookDetailsDto.NewCover.CopyToAsync(stream);
                }

                book.Cover = $"/images/{fileName}";
            }

            if (!string.IsNullOrEmpty(bookDetailsDto.Title))
                book.Title = bookDetailsDto.Title;

            if (!string.IsNullOrEmpty(bookDetailsDto.Category))
                book.Category = bookDetailsDto.Category;

            if (!string.IsNullOrEmpty(bookDetailsDto.Author))
                book.Author = bookDetailsDto.Author;

            if (!string.IsNullOrEmpty(bookDetailsDto.Summary))
                book.Summary = bookDetailsDto.Summary;

            

            _unitOfWork.Repository<BooksDatum>().Update(book);
            await _unitOfWork.CountAsync();
        }
        public async Task<IEnumerable<BookSearchResultDto>> SearchBooksAsync(string term)
        {
            term = term?.ToLower() ?? "";

            var result = await _unitOfWork.Repository<BooksDatum>().GetQueryable()
                .Where(b =>
                    (!string.IsNullOrEmpty(b.Title) && b.Title.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(b.Author) && b.Author.ToLower().Contains(term))
                )
                .Select(b => new BookSearchResultDto
                {
                    Id = b.Id ?? 0,
                    Title = b.Title,
                    Cover = b.Cover,
                    Author = b.Author,
                    Category = b.Category
                })
                .ToListAsync();

            return result;
        }




        public async Task<PaginatedBookDto> GetAllBooksAsyncUsingPaginated(int pageIndex, int pageSize)
        {
            var spec = new BookSpecification(pageIndex, pageSize);
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

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var distinctCategories = await _unitOfWork.Repository<BooksDatum>()
                .GetDistinctAsync(b => b.Category);
            return _mapper.Map<IEnumerable<CategoryDto>>(distinctCategories);
        }
    }
}