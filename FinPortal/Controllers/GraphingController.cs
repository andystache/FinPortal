using FinPortal.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FinPortal.Controllers
{
    public class GraphingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public class MorrisBarData
        {
            public string label { get; set; }
            public decimal value { get; set; }
        }

        public JsonResult ProduceChart1Data()
        {
            var userId = User.Identity.GetUserId();
            var myData = new List<MorrisBarData>();
            MorrisBarData data = null;
            foreach (var account in db.BankAccounts.Where(bA => bA.OwnerId == userId).ToList())
            {
                data = new MorrisBarData();
                data.label = account.Name;
                data.value = account.CurrentBalance;
                myData.Add(data);
            }

            return Json(myData);
        }
        public JsonResult ProduceChart2Data()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);
            var houseId = user.HouseholdId;
            var myData = new List<MorrisBarData>();
            MorrisBarData data = null;
            foreach (var budget in db.Budgets.Where(b => b.HouseholdId == houseId).ToList())
            {
                data = new MorrisBarData();
                data.label = budget.Name;
                data.value = budget.TargetAmount;
                myData.Add(data);
            }

            return Json(myData);
        }
    }
}