using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recyclica.Models;


public class RelatorioClientes
{
    [Key]
    public int IdTransacao { get; set; }

    public string NomeProduto { get; set; }
    public double Peso { get; set; }
    public DateTime Data { get; set; }

    [Required]
    public int ClienteId { get; set; }

    // Relacionamento com Cliente
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; }
}
