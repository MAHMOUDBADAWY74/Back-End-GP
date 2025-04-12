﻿using AutoMapper;
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
            string? coverUrl = null;

            if (addBookDetailsDto.Cover != null)
            {
                // التحقق من وجود عنوان للكتاب واستخدامه كجزء من اسم الملف
                var sanitizedTitle = addBookDetailsDto.Title?.Replace(" ", "_").Replace(":", "").Replace("/", "") ?? "Unknown";

                // إنشاء اسم الملف مع الامتداد الأصلي
                var fileName = $"{sanitizedTitle}_{Guid.NewGuid()}{Path.GetExtension(addBookDetailsDto.Cover.FileName)}";
                var filePath = Path.Combine("wwwroot/images", fileName);

                try
                {
                    // حفظ الصورة في المجلد المحلي
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await addBookDetailsDto.Cover.CopyToAsync(stream);
                    }

                    // إنشاء رابط URL للصورة
                    coverUrl = $"/images/{fileName}";
                }
                catch (Exception ex)
                {
                    // التعامل مع أي خطأ أثناء حفظ الصورة
                    throw new InvalidOperationException("An error occurred while saving the cover image.", ex);
                }
            }

            // إنشاء الكيان وحفظه في قاعدة البيانات
            var book = _mapper.Map<BooksDatum>(addBookDetailsDto);
            book.Cover = coverUrl; // تخزين رابط الصورة في العمود Cover

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

            // حذف الصورة من المجلد إذا كان هناك رابط للصورة
            if (!string.IsNullOrEmpty(book.Cover))
            {
                var filePath = Path.Combine("wwwroot", book.Cover.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            // حذف الكتاب من قاعدة البيانات
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

            // حذف الصورة من المجلد إذا كان هناك رابط للصورة
            if (!string.IsNullOrEmpty(book.Cover))
            {
                var filePath = Path.Combine("wwwroot", book.Cover.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // تحديث قاعدة البيانات لإزالة رابط الصورة
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



        public async  Task<BookDetailsDto> GetBookByIdAsync(long id)
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

            // تحديث صورة الغلاف إذا تم إرسال صورة جديدة
            if (bookDetailsDto.NewCover != null)
            {
                // حذف الصورة القديمة إذا كانت موجودة
                if (!string.IsNullOrEmpty(book.Cover))
                {
                    var oldFilePath = Path.Combine("wwwroot", book.Cover.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                // حفظ الصورة الجديدة
                var sanitizedTitle = bookDetailsDto.Title?.Replace(" ", "_").Replace(":", "").Replace("/", "") ?? "Unknown";
                var fileName = $"{sanitizedTitle}_{Guid.NewGuid()}{Path.GetExtension(bookDetailsDto.NewCover.FileName)}";
                var filePath = Path.Combine("wwwroot/images", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bookDetailsDto.NewCover.CopyToAsync(stream);
                }

                // تحديث رابط الصورة
                book.Cover = $"/images/{fileName}";
            }

            // تحديث الحقول الأخرى إذا تم إرسالها
            if (!string.IsNullOrEmpty(bookDetailsDto.Title))
                book.Title = bookDetailsDto.Title;

            if (!string.IsNullOrEmpty(bookDetailsDto.Category))
                book.Category = bookDetailsDto.Category;

            if (!string.IsNullOrEmpty(bookDetailsDto.Author))
                book.Author = bookDetailsDto.Author;

            if (!string.IsNullOrEmpty(bookDetailsDto.Summary))
                book.Summary = bookDetailsDto.Summary;

            if (!string.IsNullOrEmpty(bookDetailsDto.Text))
                book.Text = bookDetailsDto.Text;

            // تحديث الكتاب في قاعدة البيانات
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
