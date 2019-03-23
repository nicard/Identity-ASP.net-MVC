using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IdentityProject.ViewModels
{
    public class ContaEsqueciSenhaViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}