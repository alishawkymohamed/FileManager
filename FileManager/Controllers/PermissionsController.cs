using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DB_Project.DataBase;
using DB_Project.DataBase.Models;

namespace FileManager.Controllers
{
    public class PermissionsController : Controller
    {
        private DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();

        public ActionResult Index()
        {
            return View(db.Permissions.OrderBy(i => i.ID).ToList());
        }
        [HttpPost]
        public ActionResult RenderEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permission permission = db.Permissions.SingleOrDefault(p => p.ID == id);
            if (permission == null)
            {
                return HttpNotFound();
            }
            return PartialView("P_Edit",permission);
        }

        [HttpPost]
        public ActionResult Edit(Permission permission)
        {
            if (ModelState.IsValid)
            {
                db.Permissions.SingleOrDefault(p => p.ID == permission.ID).Name = permission.Name;
                db.SaveChanges();
                return PartialView("MainTable", db.Permissions.OrderBy(i => i.ID).ToList());
            }
            return PartialView("P_Edit", permission);
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
