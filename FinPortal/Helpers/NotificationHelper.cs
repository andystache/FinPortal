using FinPortal.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.Helpers
{
    public class NotificationHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();

        public void SendNewRoleNotification(string user, string role)
        {
            var newHead = db.Users.Find(user);
            var notification = new Notification
            {
                IsRead = false,
                HouseholdId = (int)newHead.HouseholdId,
                RecipientId = newHead.Id,
                Created = DateTime.Now,
                Subject = $"New Role Assigned",
                Body = $"You are now {role} of {newHead.Household}. You can now edit budgets for the household"
            };
            db.Notifications.Add(notification);
            db.SaveChanges();

        }
        public void SendOverBudgetNotification(string user, string budget)
        {
            var recipient = db.Users.Find(user);
            var notification = new Notification
            {
                IsRead = false,
                HouseholdId = (int)recipient.HouseholdId,
                RecipientId = recipient.Id,
                Created = DateTime.Now,
                Subject = $"Over Budget",
                Body = $"You have exceeded your {budget} budget."
            };
            db.Notifications.Add(notification);
            db.SaveChanges();

        }
        public void SendOverBudgetItemNotification(string user, string budgetItem)
        {
            var recipient = db.Users.Find(user);
            var notification = new Notification
            {
                IsRead = false,
                HouseholdId = (int)recipient.HouseholdId,
                RecipientId = recipient.Id,
                Created = DateTime.Now,
                Subject = $"Over Budget Item",
                Body = $"You have exceeded your {budgetItem} budget."
            };
            db.Notifications.Add(notification);
            db.SaveChanges();

        }


        public void SendOverdraftNotification(string user, string account)
        {
            var recipient = db.Users.Find(user);
            var notification = new Notification
            {
                IsRead = false,
                HouseholdId = (int)recipient.HouseholdId,
                RecipientId = recipient.Id,
                Created = DateTime.Now,
                Subject = $"Overdraft Notification",
                Body = $"You have overdrafted one of your {account} account"
            };
            db.Notifications.Add(notification);
            db.SaveChanges();

        }
        public void SendBalanceWarningNotification(string user, string account)
        {
            var recipient = db.Users.Find(user);
            var notification = new Notification
            {
                IsRead = false,
                HouseholdId = (int)recipient.HouseholdId,
                RecipientId = recipient.Id,
                Created = DateTime.Now,
                Subject = $"Balance Warning Notification",
                Body = $"The account {account} is below the warning threshold."
            };
            db.Notifications.Add(notification);
            db.SaveChanges();

        }

        public static List<Notification> GetUnreadNotifications()
        {
            var currentUserId = HttpContext.Current.User.Identity.GetUserId();
            return db.Notifications.Where(t => t.RecipientId == currentUserId && !t.IsRead).ToList();
        }

    }
}