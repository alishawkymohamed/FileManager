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
    public class UsersController : Controller
    {
        private DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();

        public ActionResult Index()
        {
            if (Session["UserId"] != null && Session["Role"] != null && Session["Role"].ToString() == "Admin")
            {
                return View(db.Users.Include(u => u.Role).OrderBy(u => u.ID).ToList());
            }
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("SignIn", "Home");
        }
        [HttpPost]
        public ActionResult RenderCreate()
        {
            var Roles = db.Roles.OrderBy(r => r.ID).ToList();
            Roles.Add(new Role() { ID = 0, Name = "-- Select Role --" });
            ViewBag.Roles = Roles.OrderBy(r => r.ID).ToList();
            return PartialView("P_Create");
        }

        [HttpPost]
        public ActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                if (user.RoleId == 0)
                {
                    user.Role = null;
                }
                db.Users.Add(user);
                db.SaveChanges();
                return PartialView("MainTable", db.Users.Include(u => u.Role).OrderBy(u => u.ID).ToList());
            }
            return PartialView("P_Create");
        }
        [HttpPost]
        public ActionResult RenderEdit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var UserRoles = new SelectList(db.Roles.OrderBy(r => r.ID).ToList(), "ID", "Name", db.Users.SingleOrDefault(r => r.ID == id).RoleId);
            ViewBag.UserRoles = UserRoles;
            return PartialView("P_Edit", user);
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                var oldUser = db.Users.SingleOrDefault(u => u.ID == user.ID);
                oldUser.Email = user.Email;
                oldUser.Name = user.Name;
                oldUser.Password = user.Password;
                oldUser.RoleId = user.RoleId;
                db.SaveChanges();
                return PartialView("MainTable", db.Users.Include(u => u.Role).OrderBy(u => u.ID).ToList());
            }
            return PartialView("P_Edit", user);
        }

        public ActionResult RenderDelete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return PartialView("P_Delete", user);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return PartialView("MainTable", db.Users.Include(u => u.Role).OrderBy(u => u.ID).ToList());
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
