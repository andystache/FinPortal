using FinPortal.Enums;
using FinPortal.Helpers;
using FinPortal.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace FinPortal.Extensions
{
    public static class Extensions
    {
        public static async Task RefreshAuthentication(this HttpContextBase context, ApplicationUser user)
        {
            context.GetOwinContext().Authentication.SignOut();
            await context.GetOwinContext().Get<ApplicationSignInManager>().SignInAsync(user, isPersistent: false, rememberBrowser: false);
        }
    }

    public static class InvitationExtensions
    {
        public static async Task EmailInvitation(this Invitation invitation)
        {
            var Url = new UrlHelper(HttpContext.Current.Request.RequestContext);
            var callbackUrl = Url.Action("AcceptInvitation", "Account", new { recipientEmail = invitation.RecipientEmail, code = invitation.Code }, protocol: HttpContext.Current.Request.Url.Scheme);
            var from = $"Piggy Bank <{WebConfigurationManager.AppSettings["emailfrom"]}>";

            var emailMessage = new MailMessage(from, invitation.RecipientEmail)
            {
                Subject = $"You have been invited to join the Piggy Bank Application",
                Body = $"Please accept this invitation and register as a new household member <a href=\"{callbackUrl}\">here</a><br /><br />If you have already created an account copy and paste this code in the dashboard to join the household: {invitation.Code}",
                IsBodyHtml = true
            };

            var svc = new PersonalEmail();
            await svc.SendAsync(emailMessage);
        }
    }

    public static class TransactionExtensions 
    {
        public static ApplicationDbContext db = new ApplicationDbContext();
        public static NotificationHelper notificationHelper = new NotificationHelper();


        public static void ReconcileDelete(this Transaction transaction)
        {
            ReconcileBankBalance(transaction);
            ReconcileBudgetBalance(transaction);
            ReconcileBudgetItemBalance(transaction);
        }

        public static void ReconcileEdit(this Transaction newTransaction, Transaction oldTransaction)
        {
            ReconcileBankBalance(oldTransaction);
            ReconcileBudgetBalance(oldTransaction);
            ReconcileBudgetItemBalance(oldTransaction);

            UpdateBankBalance(newTransaction);
            UpdateBudgetBalance(newTransaction);
            UpdateBudgetItemBalance(newTransaction);

        }

        public static void UpdateBalances(this Transaction transaction)
        {
            UpdateBankBalance(transaction);
            UpdateBudgetBalance(transaction);
            UpdateBudgetItemBalance(transaction);
        }

        private static void UpdateBankBalance(Transaction transaction)
        {
            var bank = db.BankAccounts.Find(transaction.BankAccountId);
            if(transaction.TransactionType == TransactionType.Deposit)
            {
                bank.CurrentBalance += transaction.Amount;
            }
            else
            {
                bank.CurrentBalance -= transaction.Amount;
                if (bank.CurrentBalance < bank.WarningBalance && bank.CurrentBalance > 0)
                    notificationHelper.SendBalanceWarningNotification(transaction.OwnerId, bank.Name);
                if(bank.CurrentBalance < 0)
                    notificationHelper.SendOverdraftNotification(transaction.OwnerId, bank.Name);
            }
            db.SaveChanges();
        }

        private static void UpdateBudgetBalance(Transaction transaction)
        {
            if(transaction.TransactionType == TransactionType.Deposit || transaction.BudgetItemId == null)
            {
                return;
            }
            var budgetItem = db.BudgetItems.Find(transaction.BudgetItemId);
            var budget = db.Budgets.Find(budgetItem.BudgetId);
            var targetAmount = budget.TargetAmount;
            budget.CurrentAmount += transaction.Amount;
            if (budget.CurrentAmount > targetAmount)
                notificationHelper.SendOverBudgetNotification(transaction.OwnerId, budget.Name);
            db.SaveChanges();
        }

        private static void UpdateBudgetItemBalance(Transaction transaction)
        {
            if (transaction.TransactionType == TransactionType.Deposit) 
            { 
                return;
            }
            var budgetItem = db.BudgetItems.Find(transaction.BudgetItemId);
            budgetItem.CurrentAmount += transaction.Amount;
            if (budgetItem.CurrentAmount > budgetItem.TargetAmount)
                notificationHelper.SendOverBudgetItemNotification(transaction.OwnerId, budgetItem.Name);
            db.SaveChanges();
        }

        private static void ReconcileBankBalance(Transaction transaction) 
        {
            var bank = db.BankAccounts.Find(transaction.BankAccountId);
            if (transaction.TransactionType == TransactionType.Deposit)
            {
                bank.CurrentBalance -= transaction.Amount;
            }
            else
            {
                bank.CurrentBalance += transaction.Amount;
            }
            db.SaveChanges();

        }
        private static void ReconcileBudgetBalance(Transaction transaction) 
        {
            if (transaction.TransactionType == TransactionType.Deposit || transaction.BudgetItemId == null)
            {
                return;
            }
            var budgetItem = db.BudgetItems.Find(transaction.BudgetItemId);
            var budget = db.Budgets.Find(budgetItem.BudgetId);
            budget.CurrentAmount -= transaction.Amount;
            db.SaveChanges();

        }
        private static void ReconcileBudgetItemBalance(Transaction transaction) 
        {
            if (transaction.TransactionType == TransactionType.Deposit || transaction.BudgetItem == null)
            {
                return;
            }
            var budgetItem = db.BudgetItems.Find(transaction.BudgetItemId);
            budgetItem.CurrentAmount -= transaction.Amount;
            db.SaveChanges();

        }

    }
}