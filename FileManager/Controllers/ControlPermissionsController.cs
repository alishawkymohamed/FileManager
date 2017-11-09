using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using DB_Project.DataBase.Models;

namespace FileManager.Controllers
{
    public class PermissionsData
    {
        public int ContentId { get; set; }
        public List<int> Permissions { get; set; }
    }
    public class SentData
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public List<PermissionsData> PermissionsData { get; set; }
    }
    public class SentModel
    {
        public User User { get; set; }
        public Role Role { get; set; }
        public List<DB_Project.DataBase.Models.Content> Contents { get; set; }
    }
    public class ControlPermissionsController : Controller
    {
        private DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();
        public ActionResult Index()
        {
            if (Session["UserId"] != null && Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                var Users = db.Users.Select(u => new { u.ID, u.Name }).ToList();
                Users.Add(new { ID = 0, Name = "-- Select User --" });
                ViewBag.Users = new SelectList(Users.OrderBy(u => u.ID).ToList(), "ID", "Name");

                var Roles = db.Roles.Select(u => new { u.ID, u.Name }).ToList();
                Roles.Add(new { ID = 0, Name = "-- Select Role --" });
                ViewBag.Roles = new SelectList(Roles.OrderBy(u => u.ID).ToList(), "ID", "Name");

                return View();
            }
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("SignIn", "Home");
        }
        [HttpPost]
        public ActionResult GetData(int? Userid, int? Roleid)
        {
            ViewBag.Permissions = db.Permissions.ToList();
            var Model = new SentModel()
            {
                Contents = db.Contents
                .Include(c => c.UserContentPermissions)
                .Include(c => c.RoleContentPermissions)
                .ToList(),
                User = db.Users.SingleOrDefault(u => u.ID == Userid),
                Role = db.Roles.SingleOrDefault(u => u.ID == Roleid)
            };
            return PartialView("mainTable", Model);
        }
        [HttpPost]
        public ActionResult SaveData(SentData dataToBeSent)
        {
            if (dataToBeSent.UserId != 0)
            {
                foreach (var item in dataToBeSent.PermissionsData)
                {
                    db.UserContentPermissions.RemoveRange(db.UserContentPermissions.Where(v => v.UserID == dataToBeSent.UserId && v.ContentID == item.ContentId).ToList());
                    if (item.Permissions != null)
                    {
                        foreach (var obj in item.Permissions)
                        {
                            db.UserContentPermissions.Add(new DB_Project.DataBase.Models.UserContentPermission() { UserID = dataToBeSent.UserId, ContentID = item.ContentId, PermissionID = obj });
                        }
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return Json(false);
                }
            }
            else
            {
                foreach (var item in dataToBeSent.PermissionsData)
                {
                    db.RoleContentPermissions.RemoveRange(db.RoleContentPermissions.Where(v => v.RoleID == dataToBeSent.RoleId && v.ContentID == item.ContentId).ToList());
                    var UsersHavingThisRole = db.Roles.SingleOrDefault(r => r.ID == dataToBeSent.RoleId).Users.ToList();
                    if (item.Permissions != null)
                    {
                        foreach (var obj in item.Permissions)
                        {
                            db.RoleContentPermissions.Add(new DB_Project.DataBase.Models.RoleContentPermission() { RoleID = dataToBeSent.RoleId, ContentID = item.ContentId, PermissionID = obj });
                        }
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return Json(false);
                }
            }

            return Json(true);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}