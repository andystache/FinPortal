using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace FinPortal.Models
{
    public class Budget
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        public string OwnerId { get; set; }
        public DateTime Created { get; set; }
        public string Name { get; set; }
        public decimal CurrentAmount { get; set; }
        public bool IsDeleted { get; set; }
        [NotMapped]
        public decimal TargetAmount
        {
            get
            {
                var target = db.BudgetItems.Where(bI => bI.BudgetId == Id).Count();
                return target != 0 ? db.BudgetItems.Where(bI => bI.BudgetId == Id).Sum(x => x.TargetAmount) : 0;
            }
        }
        public virtual Household Household { get; set; }
        public virtual ApplicationUser Owner { get; set; }
        public Budget()
        {
            BudgetItems = new HashSet<BudgetItem>();
        }
        public virtual ICollection<BudgetItem> BudgetItems { get; set; }

    }
}