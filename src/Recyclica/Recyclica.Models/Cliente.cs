using System.ComponentModel.DataAnnotations;

namespace Recyclica.Models;

public class Cliente
{
    [Key]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Campo Nome é obrigatório")]
    public string Nome { get; set; }
    public DateTime DataCadastro { get; set; }

    [Required(ErrorMessage = "Campo CPF / CNPJ é obrigatório")]
    public string? CNPJ_CPF { get; set; }

    // Relacionamento com RelatorioClientes
    //Nikolas: -Não entendi isso aqui
    //public virtual ICollection<RelatorioClientes> RelatorioClientes { get; set; }
}