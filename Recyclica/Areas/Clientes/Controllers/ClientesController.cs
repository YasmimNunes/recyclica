using Microsoft.AspNetCore.Mvc;
using Recyclica.Models;
using Recyclica.Data;
using System.Linq;

namespace Recyclica.Areas.Public.Controllers
{
    [Area("Clientes")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ClientesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Public/Clientes
        public IActionResult Index(string searchString)
        {
            var clientes = from c in _db.Clientes select c;

            // Filtra os clientes caso a busca não esteja vazia
            if (!string.IsNullOrEmpty(searchString))
            {
                clientes = clientes.Where(c => c.Nome.Contains(searchString));
            }
            clientes = clientes.OrderBy(c => c.Nome);
            return View(clientes.ToList());
        }

        // GET: /Public/Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Public/Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente newCliente)
        {
            if (ModelState.IsValid)
            {
                Console.WriteLine("Adicionando cliente: " + newCliente.Nome);
                newCliente.DataCadastro = DateTime.Now;
                _db.Clientes.Add(newCliente);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                Console.WriteLine("ModelState inválido:");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine("Erro: " + error.ErrorMessage);
                    }
                }
            }
            return View(newCliente);
        }

        // GET: /Public/Clientes/Edit/1
        public IActionResult Edit(int ClienteId)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.ClienteId == ClienteId);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: /Public/Clientes/Edit/1
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int ClienteId, Cliente updatedCliente)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.ClienteId == ClienteId);
            if (cliente == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                cliente.Nome = updatedCliente.Nome;
                // cliente.DataCadastro = updatedCliente.DataCadastro; // Atualiza a data de cadastro, se necessário
                _db.SaveChanges(); // Salva as alterações no banco
                return RedirectToAction(nameof(Index));
            }
            return View(updatedCliente);
        }

        // GET: /Public/Clientes/Delete/1
        public IActionResult Delete(int ClienteId)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.ClienteId == ClienteId);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: /Public/Clientes/Delete/1
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int ClienteId)
        {
            var cliente = _db.Clientes.FirstOrDefault(c => c.ClienteId == ClienteId);
            if (cliente != null)
            {
                _db.Clientes.Remove(cliente);
                _db.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
