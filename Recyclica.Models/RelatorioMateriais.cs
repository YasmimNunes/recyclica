using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recyclica.Models;

public class RelatorioMateriais
{
    [Key] public int IdTransacao { get; set; }

    [Required] public int MaterialId { get; set; }

    public DateTime Data { get; set; }

    // Relacionamento com Material
    [ForeignKey("MaterialId")] public virtual Material Material { get; set; }
}