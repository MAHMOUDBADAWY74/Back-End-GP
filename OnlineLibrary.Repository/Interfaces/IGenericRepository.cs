using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Interfaces
{
    public interface IGenericRepository <T> where T : BaseEntity
    {

        Task<T> GetByIdAsync(long id);


        Task<IReadOnlyList<T>> GetAllAsync();

        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
