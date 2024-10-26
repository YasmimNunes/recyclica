using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Recyclica.Services;

namespace Recyclica.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly PasswordResetService _passwordResetService;

        public ForgotPasswordModel(
            UserManager<IdentityUser> userManager,
            IEmailSender emailSender,
            PasswordResetService passwordResetService) // Injeção do serviço de e-mail e recuperação de senha
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _passwordResetService = passwordResetService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Exibe o email recebido no console do .NET
                Console.WriteLine($"Email recebido: {Input.Email}");

                var user = await _userManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ErrorMessage = "Este usuário não existe";
                    return Page();
                }

                // Usar o serviço para criar a solicitação de recuperação de senha e gerar o código
                var resetCode = await _passwordResetService.CreatePasswordResetRequestAsync(user.Id);

                // Corpo do e-mail
                var emailBody = $"<p>Seu código de recuperação de senha é: <strong>{resetCode}</strong></p>" +
                                "<p>O código expira em 24 horas.</p>";

                // Envia o e-mail com o código de recuperação
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Código de recuperação de senha - Recyclica",
                    emailBody);

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
