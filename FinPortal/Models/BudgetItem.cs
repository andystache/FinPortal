using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.Models
{
    public class BudgetItem
    {
        public int Id { get; set; }
        public int BudgetId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public int TargetAmount { get; set; }
        public int CurrentAmount { get; set; }
        public virtual Budget Budget { get; set; }

        public BudgetItem()
        {
            Transactions = new HashSet<Transaction>();
        }

        public virtual ICollection<Transaction> Transactions { get; set; }

    }
}