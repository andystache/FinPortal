using FinPortal.Models;
using FinPortal.ViewModels;
using Microsoft.AspNet.Identity;
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
            var userId = User.Identity.GetUserId();
            if (User.IsInRole("Guest"))
            {
                return View(new DashboardVM());
            };
            var user = db.Users.Where(u => u.Id == userId).FirstOrDefault();
            var houseId = (int)user.HouseholdId;
            var lastTrans = db.Transactions.OrderByDescending(t => t.Created).FirstOrDefault(t => t.OwnerId == userId);
            var dashboard = new DashboardVM
            {
                BankBalance = db.BankAccounts.Where(bA => bA.OwnerId == userId).Sum(x => x.CurrentBalance),
                BudgetTotal = db.Budgets.Where(b => b.HouseholdId == houseId).SelectMany(b => b.BudgetItems).Sum(x => x.TargetAmount),
                LastTransaction = lastTrans != null ? lastTrans.Amount : 0,
            };
            ViewBag.BankAccountId = new SelectList(db.BankAccounts.Where(b => b.OwnerId == userId), "Id", "Name");
            ViewBag.BudgetItemId = new SelectList(db.Budgets.Where(b => b.HouseholdId == houseId).SelectMany(b => b.BudgetItems), "Id", "Name");
            return View(dashboard);
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