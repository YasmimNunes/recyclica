using Microsoft.AspNetCore.Mvc;

namespace Recyclica.Areas.Gestao.Controllers
{
    [Area("Gestao")]
    public class GestaoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Editar()
        {
            return View();
        }
        public IActionResult Deletar()
        {
            return View();
        }

    }
}