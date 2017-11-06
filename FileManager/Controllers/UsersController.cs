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
            return View(db.Users.OrderBy(u => u.ID).ToList());
        }
        [HttpPost]
        public ActionResult RenderCreate()
        {
            ViewBag.Roles = db.Roles.OrderBy(r => r.ID).ToList();
            return PartialView("P_Create");
        }

        [HttpPost]
        public ActionResult Create(User user , List<int> RoleID)
        {
            if (ModelState.IsValid)
            {
                foreach (var role in RoleID)
                {
                    user.Roles.Add(db.Roles.SingleOrDefault(r => r.ID == role));
                }
                db.Users.Add(user);
                db.SaveChanges();
                return PartialView("MainTable", db.Users.OrderBy(u => u.ID).ToList());
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
            ViewBag.Roles = db.Roles.OrderBy(r => r.ID).ToList();
            var UserRoles = new MultiSelectList(db.Roles.OrderBy(r => r.ID).ToList(), "ID", "Name", db.Users.SingleOrDefault(r => r.ID == id).Roles.Select(u => u.ID));
            ViewBag.UserRoles = UserRoles;
            return PartialView("P_Edit",user);
        }

        [HttpPost]
        public ActionResult Edit(User user, IEnumerable<int> RoleID)
        {
            if (ModelState.IsValid)
            {
                var oldUser = db.Users.SingleOrDefault(u => u.ID == user.ID);
                oldUser.Email = user.Email;
                oldUser.Name = user.Name;
                oldUser.Password = user.Password;
                if (RoleID != null)
                {
                    oldUser.Roles.RemoveAll(u => 1 == 1);
                    db.SaveChanges();
                    oldUser.Roles = db.Roles.Where(r => RoleID.Contains(r.ID)).ToList();
                }
                else
                {
                    oldUser.Roles.RemoveAll(u => 1 == 1);
                }
                db.SaveChanges();
                return PartialView("MainTable", db.Users.OrderBy(u => u.ID).Include(u=>u.Roles).ToList());
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
            return PartialView("MainTable", db.Users.OrderBy(u => u.ID).ToList());
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
