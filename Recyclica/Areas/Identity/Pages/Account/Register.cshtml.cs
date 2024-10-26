using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Recyclica.Data;
using Recyclica.Models; // Certifique-se de que este namespace está correto

namespace Recyclica.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _dbContext;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _dbContext = dbContext;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public SelectList RoleList { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O Nome Completo é obrigatório.")]
            [Display(Name = "Nome Completo")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "O Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "A Senha é obrigatória.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Senha")]
            [Compare("Password", ErrorMessage = "A senha e a confirmação não correspondem.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "O Cargo é obrigatório.")]
            [Display(Name = "Cargo")]
            public string RoleId { get; set; }

            [Required(ErrorMessage = "O Número de Telefone é obrigatório.")]
            [Display(Name = "Telefone")]
            [RegularExpression(@"^\(\d{2}\) \d{5}-\d{4}$", ErrorMessage = "O número de telefone deve estar no formato (XX) XXXXX-XXXX.")]
            public string PhoneNumber { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            RoleList = new SelectList(_roleManager.Roles.OrderBy(r => r.Name).ToList(), "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Define o nome de usuário e email
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Definir o número de telefone
                user.PhoneNumber = Input.PhoneNumber;
                user.LockoutEnabled = false; // Garantir que LockoutEnabled seja false

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    // Armazena o FullName usando a propriedade sombra
                    _dbContext.Entry(user).Property("FullName").CurrentValue = Input.FullName;
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Usuário criou uma nova conta com senha.");

                    // Atribui a role ao usuário
                    var role = await _roleManager.FindByIdAsync(Input.RoleId);
                    if (role != null)
                    {
                        await _userManager.AddToRoleAsync(user, role.Name);

                        // Criar entrada em UserRolesApproval
                        var userRolesApproval = new UserRolesApproval
                        {
                            UserId = user.Id,
                            RoleId = role.Id,
                            Approved = false, // Defina como necessário
                            ApprovedBy = "",
                            ApprovalDate = DateTime.MinValue
                        };

                        _dbContext.UserRolesApproval.Add(userRolesApproval);
                        await _dbContext.SaveChangesAsync();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirme seu email",
                        $"Por favor, confirme sua conta <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicando aqui</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            RoleList = new SelectList(_roleManager.Roles.ToList(), "Id", "Name");
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Não foi possível criar uma instância de '{nameof(IdentityUser)}'. " +
                    $"Certifique-se de que '{nameof(IdentityUser)}' não é uma classe abstrata e tem um construtor sem parâmetros, ou alternativamente " +
                    $"substitua a página de registro em /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("A interface padrão requer um repositório de usuário com suporte a email.");
            }
            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}
