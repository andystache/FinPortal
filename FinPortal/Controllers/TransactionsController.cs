using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FinPortal.Enums;
using FinPortal.Helpers;
using FinPortal.Models;
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
            var transactions = db.Transactions.Include(t => t.BankAccount).Include(t => t.BudgetItem).Include(t => t.Owner);
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
            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "OwnerId");
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name");
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FirstName");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
                if(transaction.TransactionType == TransactionType.Deposit)
                {
                    transaction.BankAccount.CurrentBalance += transaction.Amount;
                    db.SaveChanges();
                }
                else
                {
                    transaction.BankAccount.CurrentBalance -= transaction.Amount;
                    transaction.BudgetItem.Budget.CurrentAmount += transaction.Amount;
                    transaction.BudgetItem.CurrentAmount += transaction.Amount;
                    db.SaveChanges();
                    if(transaction.BankAccount.CurrentBalance < 0)
                    {
                        notificationHelper.SendOverdraftNotification(userId, transaction.BankAccount.Name);
                    }
                    if(transaction.BudgetItem.Budget.CurrentAmount > transaction.BudgetItem.Budget.TargetAmount)
                    {
                        notificationHelper.SendOverBudgetNotification(userId, transaction.BudgetItem.Budget.Name);
                    }
                    if(transaction.BudgetItem.CurrentAmount > transaction.BudgetItem.TargetAmount)
                    {
                        notificationHelper.SendOverBudgetItemNotification(userId, transaction.BudgetItem.Name);
                    }
                }
                return RedirectToAction("Index");
            }

            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "OwnerId", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FirstName", transaction.OwnerId);
            return View(transaction);
        }

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
            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "OwnerId", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FirstName", transaction.OwnerId);
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
                var userId = transaction.OwnerId;

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                var newTransaction = db.Transactions.AsNoTracking().FirstOrDefault(t => t.Id == transaction.Id);

                if(oldTransaction.TransactionType == TransactionType.Deposit)
                {
                    oldTransaction.BankAccount.CurrentBalance -= oldTransaction.Amount;
                }
                else
                {
                    oldTransaction.BankAccount.CurrentBalance += oldTransaction.Amount;
                    oldTransaction.BudgetItem.Budget.CurrentAmount -= oldTransaction.Amount;
                    oldTransaction.BudgetItem.CurrentAmount -= oldTransaction.Amount;
                }
                if (newTransaction.TransactionType == TransactionType.Deposit)
                {
                    newTransaction.BankAccount.CurrentBalance += newTransaction.Amount;
                    db.SaveChanges();
                }
                else
                {
                    newTransaction.BankAccount.CurrentBalance -= newTransaction.Amount;
                    newTransaction.BudgetItem.Budget.CurrentAmount += newTransaction.Amount;
                    newTransaction.BudgetItem.CurrentAmount += newTransaction.Amount;
                    db.SaveChanges();
                    if (newTransaction.BankAccount.CurrentBalance < 0)
                    {
                        notificationHelper.SendOverdraftNotification(userId, newTransaction.BankAccount.Name);
                    }
                    if (transaction.BudgetItem.Budget.CurrentAmount > newTransaction.BudgetItem.Budget.TargetAmount)
                    {
                        notificationHelper.SendOverBudgetNotification(userId, newTransaction.BudgetItem.Budget.Name);
                    }
                    if (transaction.BudgetItem.CurrentAmount > newTransaction.BudgetItem.TargetAmount)
                    {
                        notificationHelper.SendOverBudgetItemNotification(userId, newTransaction.BudgetItem.Name);
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "OwnerId", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            ViewBag.OwnerId = new SelectList(db.Users, "Id", "FirstName", transaction.OwnerId);
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
            db.Transactions.Remove(transaction);
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
