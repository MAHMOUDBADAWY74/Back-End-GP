using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Specification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity
    {
        Task<T> GetByIdAsync(long id);
        Task<IReadOnlyList<T>> GetAllAsync();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<IReadOnlyList<T>> GetAllWithSpecAsync(ISpecification<T> spec);
        Task<int> CountWithSpecAsync(ISpecification<T> spec);
        Task<IReadOnlyList<TResult>> GetDistinctAsync<TResult>(Expression<Func<T, TResult>> selector);
        Task<IReadOnlyList<T>> GetAllWithIncludeAsync<TProperty>(Expression<Func<T, TProperty>> include);
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate); 

    }
}