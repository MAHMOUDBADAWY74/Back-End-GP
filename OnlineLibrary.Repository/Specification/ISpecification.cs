﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Repository.Specification
{
    public interface ISpecification<T>
    {

        Expression<Func<T, bool>> Criteria { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        int Take { get; }
        int Skip { get; }
        bool IsPaginated { get; }
    }
}
