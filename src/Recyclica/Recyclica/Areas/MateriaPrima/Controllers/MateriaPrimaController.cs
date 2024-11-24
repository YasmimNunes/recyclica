using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recyclica.Areas.MateriaPrima.ViewModels;
using Recyclica.Data;
using Recyclica.Models;

namespace Recyclica.Areas.MateriaPrima.Controllers
{
    [Area("MateriaPrima")]
    [Authorize]
    public class MateriaPrimaController : Controller
    {
        readonly private ApplicationDbContext _db;
        public MateriaPrimaController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            IEnumerable<Models.MateriaPrima> materiasPrima = _db.MateriaPrima;
            return View(materiasPrima);
        }
        public IActionResult MaterialCadastrado(string termoPesquisado)
        {
            IEnumerable<Material> materiais = _db.Materiais;
            if (!string.IsNullOrEmpty(termoPesquisado))
            {
                materiais = materiais.Where(m =>
                    m.Nome.Contains(termoPesquisado, StringComparison.OrdinalIgnoreCase) ||
                    m.Tipo.Contains(termoPesquisado, StringComparison.OrdinalIgnoreCase) ||
                    (m.Peso.ToString().Contains(termoPesquisado)) || // Filtra por peso
                    (m.DataEntrada.ToString("dd/MM/yyyy").Contains(termoPesquisado)) // Filtra por data
                );
            }

            return View(materiais);
        }

        [Authorize(Roles = "Gerente de operações, Operador de Piso")]
        public IActionResult CadastrarMaterial()
        {
            var viewModel = new MaterialMateriaPrimaViewModel
            {
                MateriasPrima = _db.MateriaPrima
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Gerente de operações, Operador de Piso")]
        public IActionResult CadastrarMaterial(Material material)
        {

            var viewModel = new MaterialMateriaPrimaViewModel
            {
                MateriasPrima = _db.MateriaPrima
            };

            if (material.Nome == null || material.Peso <= 0)
            {
                return View(viewModel);
            }
            else
            {
                var materiaPrima = _db.MateriaPrima.FirstOrDefault(m => m.Tipo == material.Tipo);

                materiaPrima.Peso += material.Peso;

                _db.Materiais.Add(material);
                _db.SaveChanges();

                TempData["MensagemSucesso"] = "Material cadastrado com sucesso!";

                //Exibe os erros do ModelState no console
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        foreach (var error in ModelState[key].Errors)
                        {
                            Console.WriteLine(error.ErrorMessage);
                        }
                    }

                }
                return RedirectToAction("MaterialCadastrado");
            }
        }

        [Authorize(Roles = "Gerente de operações, Operador de Piso")]
        public IActionResult Editar(int? materialId)
        {
            if (materialId == null || materialId == 0)
            {
                return NotFound();
            }
            var viewModel = new MaterialMateriaPrimaViewModel
            {
                Material = _db.Materiais.FirstOrDefault(m => m.MaterialId == materialId),
                MateriasPrima = _db.MateriaPrima
            };
            return View(viewModel);
        }
        public void verificaPesoNegativo(Models.MateriaPrima materiaPrima)
        {
            if (materiaPrima.Peso <= 0)
            {
                materiaPrima.Peso = 0;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Gerente de operações, Operador de Piso")]
        public IActionResult Editar(Material material, double pesoAntigo, string tipoAntigo)
        {

            var viewModel = new MaterialMateriaPrimaViewModel
            {
                MateriasPrima = _db.MateriaPrima
            };

            if (material.Nome == null || material.Peso < 0)
            {
                return View(viewModel);
            }
            else
            {                
                var materiaPrima = _db.MateriaPrima.FirstOrDefault(m => m.Tipo == tipoAntigo);

                materiaPrima.Peso -= pesoAntigo;

                verificaPesoNegativo(materiaPrima);

                if (material.Tipo == tipoAntigo)
                {
                    materiaPrima.Peso += material.Peso;

                    verificaPesoNegativo(materiaPrima);
                
                }
                else
                {
                    materiaPrima = _db.MateriaPrima.FirstOrDefault(m => m.Tipo == material.Tipo);
                    materiaPrima.Peso += material.Peso;

                    verificaPesoNegativo(materiaPrima);
                
                }


                _db.Materiais.Update(material);
                _db.SaveChanges();

                TempData["MensagemSucesso"] = "Material editado com sucesso!";

                //Exibe os erros do ModelState no console
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        foreach (var error in ModelState[key].Errors)
                        {
                            Console.WriteLine(error.ErrorMessage);
                        }
                    }

                }
                return RedirectToAction("MaterialCadastrado");
            }
        }
        
        [Authorize(Roles = "Gerente de operações, Operador de Piso")]
        public IActionResult Deletar(int? materialId)
        {
            if (materialId == null || materialId == null)
            {
                return NotFound();
            }

            var viewModel = new MaterialMateriaPrimaViewModel
            {
                Material = _db.Materiais.FirstOrDefault(m => m.MaterialId == materialId),
                MateriasPrima = _db.MateriaPrima
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Gerente de operações, Operador de Piso")]
        public IActionResult Deletar(Material material, double pesoAntigo, string tipo)
        {
            if (material == null)
            {
                return NotFound();
            }

            var materiaPrima = _db.MateriaPrima.FirstOrDefault(m => m.Tipo == tipo);
            materiaPrima.Peso -= pesoAntigo;

            verificaPesoNegativo(materiaPrima);

            _db.Materiais.Remove(material);
            _db.SaveChanges();

            TempData["MensagemSucessoDel"] = "Material deletado com sucesso!";

            return RedirectToAction("MaterialCadastrado");
        }
    }
}
