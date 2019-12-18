using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FinPortal.Extensions;
using FinPortal.Helpers;
using FinPortal.Models;
using FinPortal.ViewModels;
using Microsoft.AspNet.Identity;

namespace FinPortal.Controllers
{
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private RoleHelpers roleHelper = new RoleHelpers();
        private NotificationHelper notificationHelper = new NotificationHelper();

        // GET: Households
        public ActionResult Index()
        {
            return View(db.Households.ToList());
        }

        // GET: Households/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // GET: Households/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Guest")]
        public async Task<ActionResult> CreateAsync([Bind(Include = "Name,Greeting")] Household household)
        {

            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            if (ModelState.IsValid)
            {
                household.Created = DateTime.Now;
                db.Households.Add(household);
                db.SaveChanges();
                user.HouseholdId = household.Id;
                db.SaveChanges();
                roleHelper.RemoveUserFromRole(userId, "Guest");
                roleHelper.AddUserToRole(userId, "HeadOfHouse");
                await HttpContext.RefreshAuthentication(user);
                // Needs the auto logout/in to reset User Role
                // Change Redirect to Initial Account setup page
                return RedirectToAction("ConfigureHouse", "Households");
            }

            return View(household);
        }

        //GET: Configure Household
        public ActionResult ConfigureHouse()
        {
            return View(new ConfigureHouseVM());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="HeadOfHouse")]
        public ActionResult ConfigureHouse(ConfigureHouseVM model)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var houseId = (int)user.HouseholdId;

            if (ModelState.IsValid)
            {
                //Create Account
                var newAccount = new BankAccount
                {
                    HouseholdId = houseId,
                    OwnerId = userId,
                    Created = DateTime.Now,
                    Name = model.AccountName,
                    AccountType = model.AccountType,
                    StartingBalance = model.StartingBalance,
                    CurrentBalance = model.StartingBalance
                };
                db.BankAccounts.Add(newAccount);
                //Create Budget
                var newBudget = new Budget
                {
                    HouseholdId = houseId,
                    OwnerId = userId,
                    Created = DateTime.Now,
                    Name = model.BudgetName,
                    CurrentAmount = 0
                };
                db.Budgets.Add(newBudget);
                //Create Budget Item
                var newItem = new BudgetItem
                {
                    BudgetId = newBudget.Id,
                    Created = DateTime.Now,
                    Name = model.ItemName,
                    TargetAmount = model.ItemTarget,
                    CurrentAmount = 0
                };
                db.BudgetItems.Add(newItem);

                db.SaveChanges();

                return RedirectToAction("Dashboard", "Home");
            }


            //Something broke reroute back to form
            return View(model);
            
        }



        // GET: Households/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Greeting,Created")] Household household)
        {
            if (ModelState.IsValid)
            {
                db.Entry(household).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(household);
        }

        public async Task<ActionResult> LeaveAsync()
        {
            var userId = User.Identity.GetUserId();

            var myRole = roleHelper.ListUserRoles(userId).FirstOrDefault();
            var user = db.Users.Find(userId);

            switch (myRole)
            {
                case "HeadOfHouse":

                    var members = db.Users.Where(u => u.HouseholdId == user.HouseholdId).Count() - 1;
                    if(members >= 1)
                    {
                        TempData["Message"] = $"You are unable to leave the Household! There are still <b>{members}</b> other members in the house, you must select one of them to assume your role.";
                        return RedirectToAction("ExitDenied");
                    }
                    user.Household.IsDeleted = true;
                    user.HouseholdId = null;
                    db.SaveChanges();

                    roleHelper.RemoveUserFromRole(userId, "HeadOfHouse");
                    roleHelper.AddUserToRole(userId, "Guest");
                    await HttpContext.RefreshAuthentication(user);

                    return RedirectToAction("Dashboard", "Home");

                case "Member":
                default:
                    user.HouseholdId = null;
                    db.SaveChanges();

                    roleHelper.RemoveUserFromRole(userId, "Member");
                    roleHelper.AddUserToRole(userId, "Guest");
                    await HttpContext.RefreshAuthentication(user);

                    return RedirectToAction("Dashboard", "Home");
            }
        }

        [Authorize(Roles = "HeadOfHouse")]
        public ActionResult ExitDenied()
        {
            return View();
        }

        [Authorize(Roles= "HeadOfHouse")]
        public ActionResult ChangeHead()
        {
            var userId = User.Identity.GetUserId();
            var myHouseId = db.Users.Find(userId).HouseholdId ?? 0;

            if(myHouseId == 0)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var members = db.Users.Where(u => u.HouseholdId == myHouseId && u.Id != userId);
            ViewBag.NewHoH = new SelectList(members, "Id", "FullName");

            return View();
        }

        [Authorize(Roles = "HeadOfHouse")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeHeadAsync(string newHoH)
        {
            if (string.IsNullOrEmpty(newHoH))
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var me = db.Users.Find(User.Identity.GetUserId());
            me.HouseholdId = null;
            db.SaveChanges();

            roleHelper.RemoveUserFromRole(me.Id, "HeadOfHouse");
            roleHelper.AddUserToRole(me.Id, "Guest");
            await HttpContext.RefreshAuthentication(me);

            roleHelper.RemoveUserFromRole(newHoH, "Member");
            roleHelper.AddUserToRole(newHoH, "HeadOfHouse");

            //Notify the new Head of House
            notificationHelper.SendNewRoleNotification(newHoH, "Head of Household");

            return RedirectToAction("Dashboard", "Home");
        }

        // GET: Households/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Household household = db.Households.Find(id);
            household.IsDeleted = true;
            db.SaveChanges();
            return RedirectToAction("Index");
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
