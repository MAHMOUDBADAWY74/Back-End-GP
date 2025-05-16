using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserProfileService.Dtos
{
    public class CoverPhotoUpdateDto
    {
        public IFormFile CoverPhotoUpdate { get; set; }
    }
}

