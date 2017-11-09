using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileManager.Models
{
    public class LoginModelView
    {
        [Required][EmailAddress(ErrorMessage = "Please Enter Valid Email !!")]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}