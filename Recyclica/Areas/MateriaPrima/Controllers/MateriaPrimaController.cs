using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Recyclica.Areas.MateriaPrima.ViewModels;
using Recyclica.Data;
using Recyclica.Models;
using System.Collections.Generic;

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

        public IActionResult CadastrarMaterial()
        {
            var viewModel = new MaterialMateriaPrimaViewModel
            {
                MateriasPrima = _db.MateriaPrima
            };

            return View(viewModel);
        }

        public IActionResult MaterialCadastrado()
        {
            IEnumerable<Material> materiais = _db.Materiais;            
            return View(materiais);
        }

        [HttpPost]
        public IActionResult CadastrarMaterial(Material material)
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
                var materiaPrima = _db.MateriaPrima.FirstOrDefault(m => m.Tipo == material.Tipo);

                materiaPrima.Peso += material.Peso;

                _db.Materiais.Add(material);
                _db.SaveChanges();


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

        [HttpPost]
        public IActionResult Editar(Material material, double pesoAntigo)
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
                var materiaPrima = _db.MateriaPrima.FirstOrDefault(m => m.Tipo == material.Tipo);

                materiaPrima.Peso -= pesoAntigo;
                materiaPrima.Peso += material.Peso;

                _db.Materiais.Update(material);
                _db.SaveChanges();


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
        public IActionResult Deletar(Material material, double pesoAntigo, string tipo)
        {
            if (material == null)
            {
                return NotFound();
            }

            var materiaPrima = _db.MateriaPrima.FirstOrDefault(m => m.Tipo == tipo);
            materiaPrima.Peso -= pesoAntigo;
            _db.Materiais.Remove(material);
            _db.SaveChanges();

            return RedirectToAction("MaterialCadastrado");
        }
    }
}
