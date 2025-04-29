using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.CommunityService.Dtos
{
    public class CreateCommunityDto
    {
        
       
        public string Name { get; set; }
        public string Description { get; set; }

    }
}
