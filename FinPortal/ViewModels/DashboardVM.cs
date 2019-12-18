using FinPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.ViewModels
{
    public class DashboardVM
    {
        //Invitation properties
        public string Name { get; set; }
        public string Greeting { get; set; }
        public Guid Code { get; set; }

        //Transaction Creation properties
        public int BankAccountId { get; set; }
        public int BudgetItemId { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
        public int BankAccountFrom { get; set; }
        public int BankAccountTo { get; set; }


        //Card Information
        public decimal BankBalance { get; set; }
        public decimal BudgetTotal { get; set; }
        public decimal LastTransaction { get; set; }

    }
}