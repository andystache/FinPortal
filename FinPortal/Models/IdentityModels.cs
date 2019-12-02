using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FinPortal.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "First Name must be between 1 and 50 characters")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Last Name Name must be between 1 and 50 characters")]
        public string LastName { get; set; }
        public int? HouseholdId { get; set; }
        public string AvatarPath { get; set; }
        public virtual Household Household { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{LastName}, {FirstName}";
            }
        }
        public ApplicationUser()
        {
            Notifications = new HashSet<Notification>();
            BankAccounts = new HashSet<BankAccount>();
            Budgets = new HashSet<Budget>();
            Transactions = new HashSet<Transaction>();
        }
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<BankAccount> BankAccounts { get; set; }
        public virtual ICollection<Budget> Budgets { get; set; }
        public virtual ICollection<Transaction> Transactions { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<FinPortal.Models.Invitation> Invitations { get; set; }

        public System.Data.Entity.DbSet<FinPortal.Models.Household> Households { get; set; }

        public System.Data.Entity.DbSet<FinPortal.Models.BankAccount> BankAccounts { get; set; }

        public System.Data.Entity.DbSet<FinPortal.Models.Budget> Budgets { get; set; }

        public System.Data.Entity.DbSet<FinPortal.Models.BudgetItem> BudgetItems { get; set; }

        public System.Data.Entity.DbSet<FinPortal.Models.Notification> Notifications { get; set; }

        public System.Data.Entity.DbSet<FinPortal.Models.Transaction> Transactions { get; set; }

        public System.Data.Entity.DbSet<FinPortal.Models.AcceptInvitationViewModel> AcceptInvitationViewModels { get; set; }
    }
}