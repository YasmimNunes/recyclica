using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Recyclica.Data;

namespace Recyclica.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _dbContext;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger, ApplicationDbContext dbContext)
        {
            _signInManager = signInManager;
            _logger = logger;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        // Verifca se o usuário está aprovado
        private async Task<bool> UsuarioAprovado(IdentityUser user)
        {
            // Buscar approval do usuário
            var approval = await _dbContext.UserRolesApproval.FirstOrDefaultAsync(a => a.UserId == user.Id);
            if (approval != null && approval.Approved)
            {
                return true;
            }
            return false;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // Buscar o usuário por email
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
                
                // Verifca se o usuário informado é válido e se está aprovado
                if (user == null || !(await UsuarioAprovado(user)))
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado ou não aprovado.");
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Consultar a tabela AspNetUserRoles para encontrar as roles do usuário
                    var userRole = await _dbContext.UserRoles
                        .Where(ur => ur.UserId == user.Id)
                        .FirstOrDefaultAsync();

                    if (userRole != null)
                    {
                        // Consultar a tabela AspNetRoles para encontrar o nome da role associada
                        var role = await _dbContext.Roles
                            .Where(r => r.Id == userRole.RoleId)
                            .FirstOrDefaultAsync();

                        if (role != null && role.Name == "Gerente de operações")
                        {
                            // Redirecionar para a página do Gerente de Operações
                            return RedirectToAction("Index", "RoleApproval", new { area = "OperationsManager" });
                        }
                    }

                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }

            // Se chegamos aqui, algo deu errado, redisplay do formulário
            return Page();
        }
    }
}
