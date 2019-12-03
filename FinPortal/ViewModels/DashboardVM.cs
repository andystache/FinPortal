using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.ViewModels
{
    public class DashboardVM
    {
        public string Name { get; set; }
        public string Greeting { get; set; }
        public string UserId { get; set; }
        public Guid Code { get; set; }
    }
}