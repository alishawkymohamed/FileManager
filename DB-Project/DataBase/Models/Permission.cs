using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DB_Project.DataBase.Models
{
    public enum Permissions
    {
        ReadOnly = 1,
        ReadWrite = 2,
        Locked = 3,
        Hidden = 4
    }
    public class Permission
    {
        public Permission()
        {
            UserContentPermissions = new List<UserContentPermission>();
        }
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual List<UserContentPermission> UserContentPermissions { get; set; }

    }
}