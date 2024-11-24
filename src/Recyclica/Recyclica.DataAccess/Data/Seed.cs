using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Recyclica.Data;
using Recyclica.Models;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Obtenha os serviços necessários
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Defina as roles
        var roles = new[] { "Gerente de operações", "Coordenadora de Sustentabilidade", "Operador de piso", "Produção", "Diretor executivo" };

        // Crie as roles se não existirem
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole(roleName)
                {
                    NormalizedName = roleName.ToUpper()
                };
                await roleManager.CreateAsync(role);
            }
        }

        // Crie o usuário administrador
        var adminEmail = "recyclica.gerent.operacao@gmail.com";
        var adminPassword = "Abc=1234";
        var adminPhoneNumber = "(99) 99999-9999";
        var adminFullName = "Administrador";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                PhoneNumber = adminPhoneNumber,
                PhoneNumberConfirmed = true,
                LockoutEnabled = false,
                LockoutEnd = DateTimeOffset.MaxValue 
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Define o campo FullName no contexto do banco de dados
                var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var userFromDb = await userManager.FindByEmailAsync(adminEmail);

                // Atualiza o campo FullName do usuário no banco de dados
                if (userFromDb != null)
                {
                    dbContext.Entry(userFromDb).Property("FullName").CurrentValue = adminFullName;
                    await dbContext.SaveChangesAsync();
                }

                // Adiciona a role ao usuário
                await userManager.AddToRoleAsync(adminUser, "Gerente de operações");

                // Adicionar entrada em UserRolesApproval
                var role = await roleManager.FindByNameAsync("Gerente de operações");

                var userRolesApproval = new UserRolesApproval
                {
                    UserId = adminUser.Id,
                    RoleId = role.Id,
                    Approved = true,
                    ApprovedBy = "admin",
                    ApprovalDate = DateTime.Now
                };

                dbContext.UserRolesApproval.Add(userRolesApproval);
                await dbContext.SaveChangesAsync();
            }
        }

        // Adiciona as MateriaPrima base caso não existam
        var MateriasPrima = new[] { "Vidro", "Plástico", "Papel", "Alumínio", "Ferro", "Aço", "Cobre" };
        foreach (string materiaPrima in MateriasPrima)
        {
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            if (await dbContext.MateriaPrima.FirstOrDefaultAsync(m => m.Tipo == materiaPrima) == null)
            {

                var materiaPrimaPadrao = new MateriaPrima();
                materiaPrimaPadrao.Tipo = materiaPrima;
                materiaPrimaPadrao.Peso = 0;

                dbContext.MateriaPrima.Add(materiaPrimaPadrao);

                await dbContext.SaveChangesAsync();
            }
        }

    }
}
