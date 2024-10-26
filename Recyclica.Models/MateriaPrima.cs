using System.ComponentModel.DataAnnotations;

namespace Recyclica.Models;


public class MateriaPrima
{
    [Key]
    public int IdMateriaPrima { get; set; }

    public string Tipo { get; set; }
    public double Peso { get; set; }
}