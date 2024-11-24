using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Recyclica.Models;

namespace Recyclica.Models
{
    [Table("UserRolesApproval")]
    public class UserRolesApproval
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string RoleId { get; set; }

        public bool Approved { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }

        [ForeignKey("RoleId")]
        public virtual IdentityRole Role { get; set; }
    }
}