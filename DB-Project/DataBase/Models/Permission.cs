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
        Locked = 2,
        Hidden = 3
    }
    [Table("OM-Permission")]
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