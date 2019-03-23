using IdentityProject.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace IdentityProject.App_Start.OAuth
{
    public class SimpleAuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            using (var signInManager = context.OwinContext.GetUserManager<SignInManager<UsuarioAplicacao, string>>())
            {
                var UserManager = context.OwinContext.GetUserManager<UserManager<UsuarioAplicacao>>();
                var usuario = await UserManager.FindByEmailAsync(context.UserName);
                if (usuario != null)
                {
                    var signResult = await signInManager.PasswordSignInAsync(
                        usuario.UserName,
                        context.Password,
                        isPersistent: true,
                        shouldLockout: true
                        );

                    switch (signResult)
                    {
                        case SignInStatus.Success:
                            if (!usuario.EmailConfirmed)
                            {
                                context.OwinContext.Authentication.SignOut(DefaultAuthenticationTypes.ExternalBearer);
                                context.SetError("invalid_grant", "The user name or password is incorrect.");
                            }
                            else
                            {
                                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                                identity.AddClaim(new Claim("sub", context.UserName));
                                identity.AddClaim(new Claim("role", "user"));
                                context.Validated(identity);
                            }
                            break;
                        case SignInStatus.LockedOut:
                            if (await UserManager.CheckPasswordAsync(usuario, context.Password))
                                context.SetError("invalid_grant", "Conta está bloqueada");
                            else
                                context.SetError("invalid_grant", "The user name or password is incorrect.");
                            break;
                        default:
                            context.SetError("invalid_grant", "The user name or password is incorrect.");
                            break;
                    }
                }
                else
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    return;
                }

            }

        }
    }
}