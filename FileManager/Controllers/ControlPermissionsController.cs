using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
namespace FileManager.Controllers
{
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
            var Model = db.Contents.Include(c => c.UserContentPermissions).ToList();
            return PartialView("mainTable", Model);
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