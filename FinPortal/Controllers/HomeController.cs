using FinPortal.Models;
using FinPortal.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinPortal.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Dashboard()
        {
            return View(new DashboardVM());
        }

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Dismiss(int id)
        {
            var notification = db.Notifications.Find(id);
            notification.IsRead = true;
            db.SaveChanges();
            return RedirectToAction("Dashboard", "Home");
        }

    }
}