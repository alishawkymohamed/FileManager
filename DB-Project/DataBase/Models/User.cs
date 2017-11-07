using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DB_Project.DataBase.Models
{
    public class User
    {
        public User()
        {
            UserContentPermissions = new List<UserContentPermission>();
        }
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public virtual List<UserContentPermission> UserContentPermissions { get; set; }
        public virtual Role Role { get; set; }
    }
}