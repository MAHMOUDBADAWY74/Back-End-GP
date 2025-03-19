using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using OnlineLibrary.Repository.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity

    {

        private readonly OnlineLibraryIdentityDbContext _context;
        public GenericRepository(OnlineLibraryIdentityDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }


        public async Task<IReadOnlyList<T>> GetAllAsync()
         => await _context.Set<T>().ToListAsync();
         


        public async Task<T> GetByIdAsync(long id)
         => await _context.Set<T>().FindAsync(id);

        public void Update(T entity)
        => _context.Set<T>().Update(entity);


        public async Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec)
        {
            var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
            return await query.ToListAsync();
        }

        public async Task<int> CountWithSpecAsync(ISpecification<T> spec)
        {
            var query = SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
            return await query.CountAsync();
        }
    }
}
