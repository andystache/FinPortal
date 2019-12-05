using FinPortal.Enums;
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
            var from = $"Piggy Bank<{WebConfigurationManager.AppSettings["emailfrom"]}>";

            var emailMessage = new MailMessage(from, invitation.RecipientEmail)
            {
                Subject = $"You have been invited to join the Piggy Bank Application",
                Body = $"Please accept this invitation and register as a new household member <a href=\"{callbackUrl}\">here</a>",
                IsBodyHtml = true
            };

            var svc = new PersonalEmail();
            await svc.SendAsync(emailMessage);
        }
    }

    public static class TransactionExtensions 
    {
        public static ApplicationDbContext db = new ApplicationDbContext();

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
            }
            db.SaveChanges();
        }

        private static void UpdateBudgetBalance(Transaction transaction)
        {
            if(transaction.TransactionType == TransactionType.Deposit || transaction.BudgetItemId == null)
            {
                return;
            }
            else
            {
                var budget = transaction.BudgetItem.Budget;
                budget.CurrentAmount -= transaction.Amount;
                db.SaveChanges();
            }
        }

        private static void UpdateBudgetItemBalance(Transaction transaction)
        {
            if(transaction.TransactionType == TransactionType.Deposit || transaction.BudgetItem == null)
            {
                return;
            }
            else
            {
                transaction.BudgetItem.CurrentAmount -= transaction.Amount;
                db.SaveChanges();
            }
        }
    }
}