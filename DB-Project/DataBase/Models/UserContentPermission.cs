using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DB_Project.DataBase.Models
{
    public class UserContentPermission
    {
        public int ID { get; set; }
        [ForeignKey("User")]
        public int UserID { get; set; }
        [ForeignKey("Content")]
        public int ContentID { get; set; }
        [ForeignKey("Permission")]
        public int PermissionID { get; set; }
        public virtual User User { get; set; }
        public virtual Content Content { get; set; }
        public virtual Permission Permission { get; set; }
    }
}