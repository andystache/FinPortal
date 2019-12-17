using FinPortal.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int BankAccountId { get; set; }
        public int? BudgetItemId { get; set; }
        public string OwnerId { get; set; }
        public TransactionType TransactionType { get; set; }
        public DateTime Created { get; set; }
        public decimal Amount { get; set; }
        public string Memo { get; set; }
        public bool IsDeleted { get; set; }
        public virtual BankAccount BankAccount { get; set; }
        public virtual BudgetItem BudgetItem { get; set; }
        public virtual ApplicationUser Owner { get; set; }
    }
}