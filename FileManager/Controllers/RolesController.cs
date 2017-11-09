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
    public class RolesController : Controller
    {
        private DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();

        public ActionResult Index()
        {
            if (Session["UserId"] != null && Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                var Roles = db.Roles.ToList();
                Roles.Remove(db.Roles.SingleOrDefault(r => r.Name == "Admin"));
                return View(Roles.OrderBy(c => c.ID).ToList());
            }
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("SignIn", "Home");
        }
        [HttpPost]
        public ActionResult RenderCreate()
        {
            return PartialView("P_Create");
        }
        [HttpPost]
        public ActionResult Create(Role role)
        {
            if (ModelState.IsValid)
            {
                db.Roles.Add(role);
                db.SaveChanges();
                var Roles = db.Roles.ToList();
                Roles.Remove(db.Roles.SingleOrDefault(r => r.Name == "Admin"));
                return PartialView("MainTable", Roles.OrderBy(i => i.ID).ToList());
            }

            return PartialView("P_Create",role);
        }
        [HttpPost]
        public ActionResult RenderEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return PartialView("P_Edit", role);
        }
        [HttpPost]
        public ActionResult Edit(Role role)
        {
            if (ModelState.IsValid)
            {
                db.Entry(role).State = EntityState.Modified;
                db.SaveChanges();
                var Roles = db.Roles.ToList();
                Roles.Remove(db.Roles.SingleOrDefault(r => r.Name == "Admin"));
                return PartialView("MainTable", Roles.OrderBy(c => c.ID).ToList());
            }
            return PartialView("P_Edit", role);
        }
        [HttpPost]
        public ActionResult RenderDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = db.Roles.Find(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return PartialView("P_Delete", role);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Role role = db.Roles.Find(id);
            db.Roles.Remove(role);
            db.SaveChanges();
            var Roles = db.Roles.ToList();
            Roles.Remove(db.Roles.SingleOrDefault(r => r.Name == "Admin"));
            return PartialView("MainTable", Roles.OrderBy(i => i.ID).ToList());
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
