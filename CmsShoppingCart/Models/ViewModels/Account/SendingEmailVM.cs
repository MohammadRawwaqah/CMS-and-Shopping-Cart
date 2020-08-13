using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CmsShoppingCart.Models.ViewModels.Account
{
    public class SendingEmailVM
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Message { get; set; }
    }
}