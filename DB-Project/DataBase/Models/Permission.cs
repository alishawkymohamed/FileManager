﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DB_Project.DataBase.Models
{
    public class Permission
    {
        public Permission()
        {
            UserContentPermissions = new List<UserContentPermission>();
        }
        public int ID { get; set; }
        [Required]
        public string Name { get; set; }
        public virtual List<UserContentPermission> UserContentPermissions { get; set; }

    }
}