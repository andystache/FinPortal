using FinPortal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinPortal.ViewModels
{
    public class LoginRegFPVM
    {
        public LoginViewModel LoginVM { get; set; }
        public RegisterViewModel RegisterVM { get; set; }
        public ForgotPasswordViewModel ForgotPVM { get; set; }
        public LoginRegFPVM()
        {
            LoginVM = new LoginViewModel();
            RegisterVM = new RegisterViewModel();
            ForgotPVM = new ForgotPasswordViewModel();
        }
    }

}