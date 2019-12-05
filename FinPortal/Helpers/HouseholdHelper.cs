using FinPortal.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace FinPortal.Helpers
{
    public class HouseholdHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public bool IsHouseholdConfigured(string userId)
        {
            var householdId = db.Users.Find(userId).HouseholdId ?? 0;
            if (householdId == 0)
            {
                return false;
            }
            var houseHold = db.Households.Find(householdId);
            var acctCnt = houseHold.BankAccounts.Count();
            var budgetCnt = houseHold.Budgets.Count();
            var itemCnt = houseHold.Budgets.SelectMany(b => b.BudgetItems).Count();

            return (acctCnt > 0 && budgetCnt > 0 && itemCnt > 0);
        }
    }
}

