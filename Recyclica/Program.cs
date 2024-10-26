using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Recyclica.Data;
using Recyclica.Services; // Adicionar referência ao EmailSender

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ** Modificações começam aqui **
// Configurar as opções do Identity para desabilitar o lockout
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    // Desabilitar o lockout para novos usuários
    options.Lockout.AllowedForNewUsers = false;
    
    // Opcional: Você pode definir o número máximo de tentativas de acesso antes do lockout
    // Se você realmente deseja desabilitar o lockout, pode omitir estas linhas
    options.Lockout.MaxFailedAccessAttempts = 5; 
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddRoles<IdentityRole>()  // Adiciona suporte a roles
.AddEntityFrameworkStores<ApplicationDbContext>();
// ** Modificações terminam aqui **

builder.Services.AddControllersWithViews();

// Adicionar o PasswordResetService e EmailSender ao contêiner de dependência
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddTransient<IEmailSender, EmailSender>(); // Injetar o serviço de envio de e-mail

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedData.InitializeAsync(services);
}
app.UseAuthentication(); // Certifique-se de que a autenticação está sendo usada
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Public}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
