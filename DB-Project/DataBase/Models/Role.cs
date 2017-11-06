using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DB_Project.DataBase.Models
{
    public class Role
    {
        public Role()
        {
            Users = new List<User>();
        }
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual  List<User> Users { get; set; }
    }
}