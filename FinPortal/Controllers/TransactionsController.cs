using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FinPortal.Enums;
using FinPortal.Extensions;
using FinPortal.Helpers;
using FinPortal.Models;
using FinPortal.ViewModels;
using Microsoft.AspNet.Identity;

namespace FinPortal.Controllers
{
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private NotificationHelper notificationHelper = new NotificationHelper();

        // GET: Transactions
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var transactions = db.Transactions.Where(t => t.OwnerId == userId).Include(t => t.BankAccount).Include(t => t.BudgetItem).Include(t => t.Owner);
            return View(transactions.ToList());
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var houseId = user.HouseholdId;
            ViewBag.BankAccountId = new SelectList(db.BankAccounts.Where(b => b.OwnerId == userId), "Id", "Name");
            ViewBag.BudgetItemId = new SelectList(db.Budgets.Where(b => b.HouseholdId == houseId).SelectMany(b => b.BudgetItems), "Id", "Name");
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FullName");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //POST Create Withdrawal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BankAccountId,BudgetItemId,TransactionType,Amount,Memo")] Transaction transaction)
        {
            var userId = User.Identity.GetUserId();
            if (ModelState.IsValid)
            {
                transaction.OwnerId = userId;
                transaction.Created = DateTime.Now;
                db.Transactions.Add(transaction);
                db.SaveChanges();
                transaction.UpdateBalances();
                return RedirectToAction("Index");
            }

            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "Name", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FullName", transaction.OwnerId);
            return View(transaction);
        }

        // POST: Create Deposit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateDeposit(DashboardVM transaction)
        {
            var userId = User.Identity.GetUserId();
            var deposit = new Transaction()
            {
                BankAccountId = transaction.BankAccountId,
                BudgetItemId = null,
                OwnerId = userId,
                TransactionType = TransactionType.Deposit,
                Created = DateTime.Now,
                Amount = transaction.Amount,
                Memo = transaction.Memo,
                IsDeleted = false
            };
            db.Transactions.Add(deposit);
            db.SaveChanges();
            deposit.UpdateBalances();
            return RedirectToAction("Dashboard", "Home");

            //ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "Name", transaction.BankAccountId);
            //ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            //ViewBag.OwnerId = new SelectList(db.Users, "Id", "FullName", transaction.OwnerId);
            //return View(transaction);
        }

        // POST: Create Withdrawal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateWithdrawal(DashboardVM transaction)
        {
            var userId = User.Identity.GetUserId();
            var deposit = new Transaction()
            {
                BankAccountId = transaction.BankAccountId,
                BudgetItemId = transaction.BudgetItemId,
                OwnerId = userId,
                TransactionType = TransactionType.Withdrawal,
                Created = DateTime.Now,
                Amount = transaction.Amount,
                Memo = transaction.Memo,
                IsDeleted = false
            };
            db.Transactions.Add(deposit);
            db.SaveChanges();
            deposit.UpdateBalances();
            return RedirectToAction("Dashboard", "Home");
        }

        // POST: Create Transfer - Needs work on controller logic
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult CreateTransfer(DashboardVM transaction)
        //{
        //    var userId = User.Identity.GetUserId();
        //    var deposit = new Transaction()
        //    {
        //        BankAccountId = transaction.BankAccountFrom,
        //        BudgetItemId = null,
        //        OwnerId = userId,
        //        TransactionType = TransactionType.Transfer,
        //        Created = DateTime.Now,
        //        Amount = transaction.Amount,
        //        Memo = transaction.Memo,
        //        IsDeleted = false
        //    };
        //    db.Transactions.Add(deposit);
        //    db.SaveChanges();
        //    deposit.UpdateBalances();
        //    return RedirectToAction("Dashboard", "Home");
        //}


        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "Name", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FullName", transaction.OwnerId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,BankAccountId,BudgetItemId,OwnerId,TransactionType,Created,Amount,Memo")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                var oldTransaction = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == transaction.Id);

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                if(transaction.Amount != oldTransaction.Amount || transaction.BudgetItemId != oldTransaction.BudgetItemId)
                    transaction.ReconcileEdit(oldTransaction);
                return RedirectToAction("Index");
            }
            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "Name", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FullName", transaction.OwnerId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            transaction.ReconcileDelete();
            transaction.IsDeleted = true;
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
