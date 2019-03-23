using IdentityProject.Models;
using IdentityProject.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IdentityProject.Controllers
{
    public class ContaController : Controller
    {



        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                var contextowin = HttpContext.GetOwinContext();
                return contextowin.Authentication;
            }
        }


        private UserManager<UsuarioAplicacao> _userManager;
        public UserManager<UsuarioAplicacao> UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    var contextowin = HttpContext.GetOwinContext();
                    _userManager = contextowin.GetUserManager<UserManager<UsuarioAplicacao>>();
                }
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }


        private SignInManager<UsuarioAplicacao, string> _signInManager;
        public SignInManager<UsuarioAplicacao, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var contextowin = HttpContext.GetOwinContext();
                    _signInManager = contextowin.GetUserManager<SignInManager<UsuarioAplicacao, string>>();
                }
                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        public ActionResult Registrar()
        {
            return View();
        }

        public async Task<ActionResult> ConfirmacaoEmail(string usuarioId, string token)
        {

            /*var contextowin = HttpContext.GetOwinContext();
            var dbcontext = contextowin.Get<DbContext>();
            dbcontext.Database.Connection.ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;Database=ByteBank.Forum;trusted_connection=true";
            */
            if (usuarioId == null || token == null)
                return View("Error");

            var result = await UserManager.ConfirmEmailAsync(usuarioId, token);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("Error");
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(ContaRegistrarViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);
                var userExist = usuario != null;

                if (userExist)
                {
                    return View("AguardandoConfirmacao");
                }

                var novoUsuario = new UsuarioAplicacao();
                novoUsuario.Email = modelo.Email;
                novoUsuario.UserName = modelo.UserName;
                novoUsuario.NomeCompleto = modelo.NomeCompleto;
                var result = await UserManager.CreateAsync(novoUsuario, modelo.Senha);

                if (result.Succeeded)
                {
                    await EnviarEmailDeConfirmacaoAsync(novoUsuario);
                    return View("AguardandoConfirmacao");
                }
                else
                {
                    AdicionaError(result);
                }
            }

            // Alguma coisa de errado aconteceu!
            return View(modelo);
        }

        private async Task EnviarEmailDeConfirmacaoAsync(UsuarioAplicacao usuario)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(usuario.Id);

            var linkCallback = Url.Action(
                "ConfirmacaoEmail",
                "Conta",
                new
                {
                    usuarioId = usuario.Id,
                    token = token
                },
                Request.Url.Scheme
                );

            var body = "Seu token de Acesso: " + linkCallback;

            await UserManager.SendEmailAsync(usuario.Id, "Confirmação de E-mail", body);
        }

        private void AdicionaError(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public async Task<ActionResult> EsqueciSenha()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> EsqueciSenha(ContaEsqueciSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);
                if (usuario != null)
                {
                    var token = await UserManager.GeneratePasswordResetTokenAsync(usuario.Id);
                    var linkCallback = Url.Action(
                    "ConfirmacaoAlteracaoSenha",
                    "Conta",
                    new
                    {
                        usuarioId = usuario.Id,
                        token = token
                    },
                    Request.Url.Scheme
                    );

                    var body = "Alteração de senha : " + linkCallback;

                    await UserManager.SendEmailAsync(usuario.Id, "Alteração de senha : ", body);
                }
                return View("EmailALteracaoSenhaEnviado");
            }
            return View();
        }

        public async Task<ActionResult> ConfirmacaoAlteracaoSenha(string usuarioId, string token)
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmacaoAlteracaoSenha(ContaConfirmacaoAlteracaoSenhaViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var alteracao = await UserManager.ResetPasswordAsync(
                    modelo.UsuarioId,
                    modelo.Token,
                    modelo.NovaSenha
                    );
                if (alteracao.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                AdicionaError(alteracao);
            }
            return View();
        }

        public async Task<ActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<ActionResult> Login(ContaLoginViewModel modelo)
        {
            if (ModelState.IsValid)
            {
                var usuario = await UserManager.FindByEmailAsync(modelo.Email);
                if (usuario != null)
                {
                    var signResult = await SignInManager.PasswordSignInAsync(
                        usuario.UserName,
                        modelo.Senha,
                        isPersistent: modelo.ContinuarLogado,
                        shouldLockout: true
                        );

                    switch (signResult)
                    {
                        case SignInStatus.Success:
                            if (!usuario.EmailConfirmed)
                            {
                                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                                return View("AguardandoConfirmacao");
                            }
                            return RedirectToAction("Index", "Home");
                        case SignInStatus.LockedOut:
                            if (await UserManager.CheckPasswordAsync(usuario, modelo.Senha))
                                ModelState.AddModelError("", "Conta está bloqueada");
                            else
                                return SenhaOuUsuarioInvalidos();
                            break;
                        default:
                            return SenhaOuUsuarioInvalidos();
                    }
                }
                else
                    return SenhaOuUsuarioInvalidos();
            }
            return View();
        }

        private ActionResult SenhaOuUsuarioInvalidos()
        {
            ModelState.AddModelError("", "Credenciais inválidas");
            return View("Login");
        }
    }
}