using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserService.Dtos
{
    public class RequestEditUserDto
    {
        public string UserId { get; set; }
        public string FieldName { get; set; }
        public string NewValue { get; set; }
    }
}
