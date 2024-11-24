using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Recyclica.Data;
using Recyclica.Models;
using Microsoft.EntityFrameworkCore;

namespace Recyclica.Areas.Identity.Pages.Account
{
    public class ForgotPasswordConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public ForgotPasswordConfirmationModel(UserManager<IdentityUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Code { get; set; } // O código de 6 dígitos.

            [Required]
            [DataType(DataType.Password)]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "As senhas não coincidem.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Verificar se o código está correto e dentro do prazo de validade
                var resetRequest = await _dbContext.PasswordResetRequests
                    .Where(r => r.ResetCode == Input.Code && r.Status == "Pending" && r.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(r => r.CreatedAt) // Pegar a solicitação mais recente
                    .FirstOrDefaultAsync();

                if (resetRequest == null)
                {
                    // Se o código não for encontrado ou expirou
                    ModelState.AddModelError(string.Empty, "O código de recuperação é inválido ou expirou.");
                    return Page();
                }

                // Encontrar o usuário associado ao código de recuperação
                var user = await _userManager.FindByIdAsync(resetRequest.UserId);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "O usuário associado ao código não foi encontrado.");
                    return Page();
                }

                // Redefinir a senha diretamente
                var hashedPassword = _userManager.PasswordHasher.HashPassword(user, Input.NewPassword);
                user.PasswordHash = hashedPassword;

                // Atualizar o status da solicitação de recuperação de senha para "Used"
                resetRequest.Status = "Used";
                _dbContext.PasswordResetRequests.Update(resetRequest);

                // Atualizar o usuário com a nova senha
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                return RedirectToPage("./Login");
            }

            return Page();
        }
    }
}
