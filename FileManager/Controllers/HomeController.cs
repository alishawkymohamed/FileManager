using FileManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileManager.Controllers
{
    public class HomeController : Controller
    {
        private DB_Project.DataBase.FileManager db = new DB_Project.DataBase.FileManager();
        public virtual ActionResult Index(string subFolder)
        {
            if (Session["UserId"] != null)
            {
                FileViewModel model = new FileViewModel { Folder = "Files", SubFolder = subFolder };
                return View(model);
            }
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("SignIn","Home");
        }
        public ActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult PerformSignIn(string email,string pass)
        {
            var user = db.Users.SingleOrDefault(u => u.Email.ToLower() == email.ToLower());
            if (user != null)
            {
                if(user.Password == pass)
                {
                    Session.Add("UserId", user.ID);
                    if (user.Role.Name == "Admin")
                    {
                        Session.Add("Role", "Admin");
                    }
                    return Json(true);
                }
                return Json(false);
            }
            return Json(false);
        }
        public ActionResult GoHome()
        {
            return View();
        }
        public ActionResult SignOut()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("SignIn");
        }
    }
}