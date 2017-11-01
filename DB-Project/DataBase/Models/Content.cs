using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DB_Project.DataBase.Models
{
    public enum Type{ File, Folder }
    public class Content
    {
        public Content()
        {
            UserContentPermissions = new List<UserContentPermission>();
        }
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public Type Type { get; set; }
        public virtual List<UserContentPermission> UserContentPermissions { get; set; }

    }
}