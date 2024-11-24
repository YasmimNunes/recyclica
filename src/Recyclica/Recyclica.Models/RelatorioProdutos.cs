using System.ComponentModel.DataAnnotations;

namespace Recyclica.Models;

public class RelatorioProdutos
{
    [Key] public int IdRelatorioProduto { get; set; }

    public string? NomeProduto { get; set; }
    public string? Tipo { get; set; }
    public double? Peso { get; set; }
    public string? Cliente { get; set; }
    public DateTime? DataEntrada { get; set; }
    public DateTime? DataSaida { get; set; } = DateTime.Now;
}