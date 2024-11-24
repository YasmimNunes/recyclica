using Microsoft.AspNetCore.Mvc;
using Recyclica.Models;

namespace Recyclica.Areas.MateriaPrima.ViewModels
{
    [Area("MateriaPrima")]
    public class MaterialMateriaPrimaViewModel
    {
        public Material Material { get; set; }
        public IEnumerable<Models.MateriaPrima> MateriasPrima { get; set; }
    }
}
