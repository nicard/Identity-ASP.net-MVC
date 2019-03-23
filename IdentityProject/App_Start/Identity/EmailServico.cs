using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace IdentityProject.App_Start.Identity
{
    public class EmailServico : IIdentityMessageService
    {

        private readonly string EMAIL_ORIGEM = ConfigurationManager.AppSettings["emailServico:email_remetente"];
        private readonly string EMAIL_SENHA = ConfigurationManager.AppSettings["emailServico:email_senha"];

        public async Task SendAsync(IdentityMessage message)
        {
            using (var menssagemDeEmail = new MailMessage())
            {
                menssagemDeEmail.From = new MailAddress("rra_ncd@hotmail.com");

                menssagemDeEmail.Subject = message.Subject;
                menssagemDeEmail.To.Add(message.Destination);
                menssagemDeEmail.Body = message.Body;

                using(var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new NetworkCredential(EMAIL_ORIGEM, EMAIL_SENHA);

                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Host = "smtp.mailtrap.io";
                    smtpClient.Port = 2525;
                    smtpClient.EnableSsl = true;

                    smtpClient.Timeout = 20_000;

                    await smtpClient.SendMailAsync(menssagemDeEmail);
                }
            }
        }
    }
}