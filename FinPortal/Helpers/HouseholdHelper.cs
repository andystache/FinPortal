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

        public bool IsUserInHouse(string userId, int householdId)
        {
            var household = db.Households.Find(householdId);
            var flag = household.Users.Any(u => u.Id == userId);
            return (flag);
        }


        public void AddUserToHouse(string userId, int householdId)
        {
            if (!IsUserInHouse(userId, householdId))
            {
                Household house = db.Households.Find(householdId);
                var newUser = db.Users.Find(userId);

                house.Users.Add(newUser);
                db.SaveChanges();
            }
        }

        public void RemoveUserFromHouse(string userId, int householdId)
        {
            if (IsUserInHouse(userId, householdId))
            {
                Household house = db.Households.Find(householdId);
                var delUser = db.Users.Find(userId);

                house.Users.Remove(delUser);
                db.Entry(house).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }
}

