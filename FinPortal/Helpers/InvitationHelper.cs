using FinPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.Helpers
{
    public static class InvitationHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        public static void MarkAsInvalid(int id)
        {
            var invitation = db.Invitations.Find(id);
            invitation.IsValid = false;

            db.SaveChanges();
        }
    }
}