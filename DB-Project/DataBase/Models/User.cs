using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DB_Project.DataBase.Models
{
    public class User
    {
        public User()
        {
            UserContentPermissions = new List<UserContentPermission>();
            Roles = new List<Role>();
        }
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public virtual List<UserContentPermission> UserContentPermissions { get; set; }
        public virtual List<Role> Roles { get; set; }
    }
}