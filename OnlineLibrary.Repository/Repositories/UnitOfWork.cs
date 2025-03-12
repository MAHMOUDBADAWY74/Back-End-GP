using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Contexts;
using OnlineLibrary.Data.Entities;
using OnlineLibrary.Repository.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OnlineLibraryIdentityDbContext _context;

        private Hashtable _repositories;
        public UnitOfWork(OnlineLibraryIdentityDbContext context)
        {
            _context = context;
        }
        public async Task<int> CountAsync()

         => await _context.SaveChangesAsync();

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            if (_repositories == null)
            {
                _repositories = new Hashtable();
            }

            var entityKey = typeof(T).Name;

            if (!_repositories.ContainsKey(entityKey))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);

                _repositories.Add(entityKey, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[entityKey];
        }
    }
}
