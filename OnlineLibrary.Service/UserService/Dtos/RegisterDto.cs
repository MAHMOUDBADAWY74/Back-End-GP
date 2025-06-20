﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineLibrary.Service.UserService.Dtos
{
    public class RegisterDto
    {


        [Required]
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string UserName { get; set; } 

        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}

