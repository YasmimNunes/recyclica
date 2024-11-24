using System.ComponentModel.DataAnnotations;

namespace Recyclica.Models;

public class Produto
{
    [Key]
    public int ProdutoId { get; set; }

    public string? NomeProduto { get; set; }

    public string? Tipo { get; set; }
    public double? Peso { get; set; }
    public DateTime? DataEntrada { get; set; }
    public DateTime? DataSaida { get; set; }
}

