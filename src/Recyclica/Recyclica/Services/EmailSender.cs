using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;

namespace Recyclica.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Configurações de envio de e-mail usando SMTP
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("recyclica.gerent.operacao@gmail.com", "sqje tpdf ywod rqoy"), // Email e senha do Gmail
                EnableSsl = true,
            };

            // Compondo a mensagem de e-mail
            var mailMessage = new MailMessage
            {
                From = new MailAddress("recyclica.gerent.operacao@gmail.com", "Recyclica Operação"), // Endereço e nome do remetente
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true, // Permitir HTML no corpo do e-mail
            };

            mailMessage.To.Add(email); // Destinatário

            // Enviar e-mail de forma assíncrona
            return smtpClient.SendMailAsync(mailMessage);
        }
    }
}