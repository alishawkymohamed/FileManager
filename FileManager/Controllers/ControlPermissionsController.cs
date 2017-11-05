using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
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
        public List<PermissionsData> PermissionsData { get; set; }
    }
    public class SentModel
    {
        public int UserId { get; set; }
        public List<DB_Project.DataBase.Models.Content> Contents { get; set; }
    }
    public class ControlPermissionsController : Controller
    {
        private DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();
        public ActionResult Index()
        {
            var Users = db.Users.Select(u => new { u.ID, u.Name }).ToList();
            Users.Add(new { ID = 0, Name = "-- Select User --" });
            ViewBag.Users = new SelectList(Users.OrderBy(u => u.ID).ToList(), "ID", "Name");

            return View();
        }
        [HttpPost]
        public ActionResult GetData(int? id)
        {
            ViewBag.Permissions = db.Permissions.ToList();
            var Model = new SentModel()
            {
                Contents = db.Contents
                .Include(c => c.UserContentPermissions)
                .ToList(),
                UserId = (int)id
            };
            return PartialView("mainTable", Model);
        }
        [HttpPost]
        public ActionResult SaveData(SentData dataToBeSent)
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