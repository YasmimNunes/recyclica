using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Recyclica.Models;

public class Material
{
    [Key] public int MaterialId { get; set; }

    [Required (ErrorMessage = "Digite o nome do Material")]
    public string Nome { get; set; }
    public string Tipo { get; set; }
    
    [Required(ErrorMessage = "Digite um peso válido em Kg para o material")]
    [Range(1, double.MaxValue, ErrorMessage ="Apenas valores positivos maiores que 0 são permitidos")]
    public double Peso { get; set; }
    public DateTime DataEntrada { get; set; } = DateTime.Now;

    [Required] public string UserId { get; set; }

    // Relacionamento com o usuário
    [ForeignKey("UserId")] public virtual IdentityUser User { get; set; }

    // Relacionamento com RelatorioMateriais
    public virtual ICollection<RelatorioMateriais> RelatorioMateriais { get; set; }
}