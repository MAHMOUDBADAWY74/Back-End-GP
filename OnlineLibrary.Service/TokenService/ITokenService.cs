﻿using OnlineLibrary.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.TokenService
{
    public interface ITokenService
    {
        string GenerateToken(ApplicationUser applicationUser);
    }
}
