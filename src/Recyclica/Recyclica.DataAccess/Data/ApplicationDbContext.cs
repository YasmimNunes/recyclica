using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Recyclica.Models;

namespace Recyclica.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet para as tabelas adicionais
        public DbSet<UserRolesApproval> UserRolesApproval { get; set; }
        public DbSet<Material> Materiais { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<MateriaPrima> MateriaPrima { get; set; }
        public DbSet<RelatorioMateriais> RelatorioMateriais { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<RelatorioClientes> RelatorioClientes { get; set; }
        public DbSet<RelatorioProdutos> RelatorioProdutos { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuração da tabela UserRolesApproval
            builder.Entity<UserRolesApproval>(entity =>
            {
                entity.ToTable("UserRolesApproval");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Approved).HasDefaultValue(false);
            });

            // Configuração da propriedade sombra FullName na entidade IdentityUser
            builder.Entity<IdentityUser>(entity =>
            {
                entity.Property<string>("FullName").HasMaxLength(256);
            });

            // Configurações adicionais podem ser feitas aqui
        }
    }
}