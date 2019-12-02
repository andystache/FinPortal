using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.Models
{
    public class Household
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Greeting { get; set; }
        public DateTime Created { get; set; }

        public Household()
        {
            Users = new HashSet<ApplicationUser>();
            BankAccounts = new HashSet<BankAccount>();
            Invitations = new HashSet<Invitation>();
            Notifications = new HashSet<Notification>();
            Budgets = new HashSet<Budget>();
        }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<BankAccount> BankAccounts { get; set; }
        public virtual ICollection<Invitation> Invitations { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }

    }
}