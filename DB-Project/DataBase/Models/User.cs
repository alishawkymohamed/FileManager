using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DB_Project.DataBase.Models
{
    [Table("OM-User")]
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
        [EmailAddress(ErrorMessage = "Please Enter Valid Email !!")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        public virtual List<UserContentPermission> UserContentPermissions { get; set; }
        public virtual Role Role { get; set; }
    }
}