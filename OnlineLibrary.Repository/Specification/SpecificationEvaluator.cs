using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Specification
{
    public class SpecificationEvaluator<T> where T : BaseEntity
    {

        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;

           
            if (spec.Criteria != null)
            {
                query = query.Where(spec.Criteria);
            }

          
            query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

           
            if (spec.IsPaginated)
            {
                query = query.Skip(spec.Skip).Take(spec.Take);
            }

            return query;
        }
    }
}
