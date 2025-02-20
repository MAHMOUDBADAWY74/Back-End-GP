using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserService.Dtos
{
    public class ResetPasswordDto
    
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }

}
