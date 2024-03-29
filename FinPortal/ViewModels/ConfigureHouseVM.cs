﻿using FinPortal.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FinPortal.ViewModels
{
    public class ConfigureHouseVM
    {

        //Bank Account
        [Required(ErrorMessage = "Please enter a name")]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Please select an account type")]
        public AccountType AccountType { get; set; }

        [Required(ErrorMessage = "Please enter a value")]
        public decimal StartingBalance { get; set; }

        [Required(ErrorMessage = "Please enter a value")]
        public decimal WarningBalance { get; set; }

        //Budget
        [Required(ErrorMessage = "Please enter a name")]
        public string BudgetName { get; set; }

        //BudgetItems
        [Required(ErrorMessage = "Please enter a name")]
        public string ItemName { get; set; }

        [Required(ErrorMessage = "Please enter a value")]
        public decimal ItemTarget { get; set; }


    }
}