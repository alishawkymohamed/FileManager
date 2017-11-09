using DB_Project.DataBase.Models;
using ElFinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileManager.Controllers
{
    public class FileController : Controller
    {
        private DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();
        public virtual ActionResult Index(string folder, string subFolder)
        {
            var UserId = int.Parse(Session["UserId"].ToString());
            var UserRoleID = db.Users.SingleOrDefault(u => u.ID == UserId).RoleId;
            var driver = new FileSystemDriver();
            var Root = db.Contents.SingleOrDefault(f => f.Name == "Root Folder" && f.Path == "~" && f.Type == DB_Project.DataBase.Models.Type.Folder);
            DB_Project.DataBase.Models.UserContentPermission UserLockedPermission = null;
            DB_Project.DataBase.Models.UserContentPermission UserReadOnlyPermission = null;
            DB_Project.DataBase.Models.RoleContentPermission RoleLockedPermission = null;
            DB_Project.DataBase.Models.RoleContentPermission RoleReadOnlyPermission = null;
            if (Root != null)
            {
                if (db.UserContentPermissions.Where(r => r.UserID == UserId).Count() > 0)
                {
                    UserLockedPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == Root.ID && v.PermissionID == (int)Permissions.Locked);
                    RoleLockedPermission = null;//db.RoleContentPermissions.SingleOrDefault(v => UserRolesID == (v.RoleID) && v.ContentID == CurrentFile.ID && v.PermissionID == (int)Permissions.Hidden);
                    UserReadOnlyPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == Root.ID && v.PermissionID == (int)Permissions.ReadOnly);
                    RoleReadOnlyPermission = null;//db.RoleContentPermissions.SingleOrDefault(v => db.Users.SingleOrDefault(u => u.ID == UserId).RoleId == (v.RoleID) && v.ContentID == Root.ID && v.PermissionID == (int)Permissions.ReadOnly);
                }
                else
                {
                    UserLockedPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == Root.ID && v.PermissionID == (int)Permissions.Locked);
                    RoleLockedPermission = db.RoleContentPermissions.SingleOrDefault(v => UserRoleID == (v.RoleID) && v.ContentID == Root.ID && v.PermissionID == (int)Permissions.Locked);
                    UserReadOnlyPermission = db.UserContentPermissions.SingleOrDefault(v => v.UserID == UserId && v.ContentID == Root.ID && v.PermissionID == (int)Permissions.ReadOnly);
                    RoleReadOnlyPermission = db.RoleContentPermissions.SingleOrDefault(v => UserRoleID == (v.RoleID) && v.ContentID == Root.ID && v.PermissionID == (int)Permissions.ReadOnly);
                }
            }

            var root = new Root(new DirectoryInfo(Server.MapPath("~/Content/" + folder)),
                "http://" + Request.Url.Authority + "/Content/" + folder + "/")
            {
                IsLocked = ((UserLockedPermission != null) || (UserLockedPermission != null || RoleLockedPermission != null)),
                IsReadOnly = ((UserReadOnlyPermission != null) || (UserReadOnlyPermission != null || RoleReadOnlyPermission != null)),
                Alias = "Files",
                MaxUploadSizeInMb = 500
            };

            if (!string.IsNullOrEmpty(subFolder))
            {
                root.StartPath = new DirectoryInfo(Server.MapPath("~/Content/" + folder + "/" + subFolder));
            }

            driver.AddRoot(root);
            var connector = new Connector(driver);
            return connector.Process(HttpContext.Request);
        }

        public virtual ActionResult SelectFile(string target)
        {
            var driver = new FileSystemDriver();

            driver.AddRoot(
                new Root(
                    new DirectoryInfo(Server.MapPath("~/Content")),
                    "http://" + Request.Url.Authority + "/Content")
                { IsReadOnly = false });

            var connector = new Connector(driver);

            return Json(connector.GetFileByHash(target).FullName);
        }
    }
}