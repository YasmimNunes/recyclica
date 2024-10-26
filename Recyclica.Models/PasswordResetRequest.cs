using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Recyclica.Models
{
    public class PasswordResetRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey("AspNetUsers")] // Chave estrangeira para a tabela de usuários
        public string UserId { get; set; }

        [Required]
        [StringLength(6)] // Código de 6 dígitos
        public string ResetCode { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } // Data da solicitação

        [Required]
        public DateTime ExpiresAt { get; set; } // Data de expiração

        [Required]
        [StringLength(20)]
        public string Status { get; set; } // Status da solicitação, ex: "Pending", "Used", "Expired"
        
        // Propriedade de navegação opcional para o usuário
        public virtual IdentityUser User { get; set; }
    }
}