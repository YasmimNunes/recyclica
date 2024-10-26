using System.ComponentModel.DataAnnotations;

namespace Recyclica.Models;

public class Cliente
{
    [Key]
    public int ClienteId { get; set; }

    public string Nome { get; set; }
    public DateTime DataCadastro { get; set; }

    // Relacionamento com RelatorioClientes
    //Nikolas: -Não entendi isso aqui
    //public virtual ICollection<RelatorioClientes> RelatorioClientes { get; set; }
}