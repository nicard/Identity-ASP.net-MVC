using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IdentityProject.ViewModels
{
    public class ContaConfirmacaoAlteracaoSenhaViewModel
    {
        [Required]
        public string UsuarioId;

        [Required]
        public string Token;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string NovaSenha;
    }
}