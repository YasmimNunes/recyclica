using Microsoft.AspNetCore.Mvc;
using Recyclica.Data;
using Recyclica.Models;

namespace Recyclica.Areas.Produtos.Controllers
{
    [Area("Produtos")]
    public class ProdutosController : Controller
    {
        readonly private ApplicationDbContext _db;

        public ProdutosController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index(string searchTerm)
        {
            IEnumerable<Produto> produtos = _db.Produtos.ToList();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                produtos = produtos.Where(p =>
                    p.NomeProduto.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    p.Tipo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (p.Peso.HasValue && p.Peso.Value.ToString().Contains(searchTerm)) || // Filtra por peso
                    (p.DataEntrada.HasValue && p.DataEntrada.Value.ToString("dd/MM/yyyy").Contains(searchTerm)) // Filtra por data
                );
            }

            return View(produtos);
        }


        public IActionResult Editar(int id)
        {
            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoId == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }
        [HttpPost]
        public IActionResult Atualizar(Produto produto)
        {
            if (ModelState.IsValid)
            {
                // Encontra o produto existente no banco de dados pelo ID
                var produtoExistente = _db.Produtos.FirstOrDefault(p => p.ProdutoId == produto.ProdutoId);
                if (produtoExistente != null)
                {
                    // Atualiza apenas os campos que podem ser alterados
                    produtoExistente.NomeProduto = produto.NomeProduto;
                    produtoExistente.Tipo = produto.Tipo;
                    produtoExistente.Peso = produto.Peso;

                    // Mantém o valor de DataEntrada existente
                    produtoExistente.DataEntrada = produtoExistente.DataEntrada;

                    // Salva as alterações
                    _db.SaveChanges();

                    // Redireciona para a página de índice
                    return RedirectToAction("Index");
                }
                else
                {
                    return NotFound(); // Se o produto não for encontrado
                }
            }

            // Se houver erro de validação, retorna a view com o produto atual
            return View(produto);
        }

        public IActionResult Vender(int id)
        {
            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoId == id);
            if (produto == null)
            {
                return NotFound(); // Retorna 404 se o produto não for encontrado
            }

            // Carrega a lista de clientes
            ViewBag.Clientes = _db.Clientes.Select(c => new
            {
                ClienteId = c.ClienteId,
                Nome = c.Nome
            }).ToList();

            return View(produto); // Retorna a view com o produto
        }

        [HttpPost]
        public IActionResult RegistrarVenda(int produtoId, int clienteId, string nomeProduto, string tipo, double peso, DateTime? dataEntrada)
        {
            // Recupera o produto do banco de dados
            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoId == produtoId);
            if (produto == null)
            {
                return NotFound();
            }

            // Recupera o nome do cliente
            var cliente = _db.Clientes.FirstOrDefault(c => c.ClienteId == clienteId);
            if (cliente == null)
            {
                ModelState.AddModelError("", "Cliente não encontrado.");
                return View("Vender", produto);
            }

            // Verifica se o peso a ser debitado não é maior que o peso disponível
            if (peso > produto.Peso)
            {
                ModelState.AddModelError("", "O peso a ser debitado não pode ser maior que o peso disponível.");
                return View("Vender", produto);
            }

            // Debita o peso do produto
            produto.Peso -= peso;

            // Se o peso do produto for zero, exclui o produto
            if (produto.Peso <= 0)
            {
                _db.Produtos.Remove(produto);
            }

            // Salva as alterações no produto
            _db.SaveChanges();

            // Cria um novo relatório de produto
            var relatorioProduto = new RelatorioProdutos
            {
                NomeProduto = nomeProduto,
                Tipo = tipo,
                Peso = peso,
                DataEntrada = dataEntrada,
                DataSaida = DateTime.Now,
                Cliente = cliente.Nome // Usa o nome do cliente recuperado
            };

            _db.RelatorioProdutos.Add(relatorioProduto);
            _db.SaveChanges();

            // Redireciona para a ação Index
            return RedirectToAction("Index");
        }



        public IActionResult Deletar(int id)
        {
            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoId == id);
            if (produto == null)
            {
                return NotFound();
            }

            return View(produto);
        }

        [HttpPost]
        public IActionResult ConfirmarDeletar(int id)
        {
            var produto = _db.Produtos.FirstOrDefault(p => p.ProdutoId == id);
            if (produto != null)
            {
                _db.Produtos.Remove(produto);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            return NotFound();
        }

        public IActionResult Relatorio()
        {
            // Obtém os registros da tabela RelatorioProdutos
            var relatorios = _db.RelatorioProdutos.ToList();

            // Retorna a view e passa os dados
            return View("Relatorio", relatorios);
        }

        public IActionResult NovoProduto()
        {
            var materiasPrimas = _db.MateriaPrima.Where(mp => mp.Peso > 0).ToList();
            ViewBag.MateriasPrimas = materiasPrimas; // Passando a lista diretamente
            return View();
        }




        [HttpPost]
        public IActionResult NovoProduto(Produto novoProduto)
        {
            // Verifica se os dados do modelo estão válidos
            if (!ModelState.IsValid)
            {
                return View(novoProduto); // Retorna a view com o modelo atual se não for válido
            }

            // Verifica se um produto com o mesmo NomeProduto já existe
            var produtoExistente = _db.Produtos
                .FirstOrDefault(p => p.NomeProduto == novoProduto.NomeProduto);

            // Obtém a matéria-prima correspondente
            var materiaPrima = _db.MateriaPrima
                .FirstOrDefault(mp => mp.Tipo == novoProduto.Tipo);

            // Verifica se a matéria-prima foi encontrada
            if (materiaPrima == null)
            {
                ModelState.AddModelError("", "Matéria-prima não encontrada.");
                return View(novoProduto);
            }

            // Valida se o Peso não é nulo e se é menor ou igual ao Peso da matéria-prima
            if (novoProduto.Peso <= 0 || novoProduto.Peso > materiaPrima.Peso)
            {
                ModelState.AddModelError("", "O peso do produto deve ser maior que 0 e não pode ser maior que o peso em estoque.");
                return View(novoProduto); // Retorna à view para corrigir o erro
            }

            // Se o produto existe, atualiza o Peso
            if (produtoExistente != null)
            {
                produtoExistente.Peso += novoProduto.Peso;
                produtoExistente.DataEntrada = DateTime.Now; // Atualiza a DataEntrada
                _db.Produtos.Update(produtoExistente);
            }
            else
            {
                // Se o produto não existe, cria um novo
                novoProduto.DataEntrada = DateTime.Now; // Definindo a Data de Fabricação
                novoProduto.DataSaida = new DateTime(9999, 1, 1); // Definindo a Data de Saída
                _db.Produtos.Add(novoProduto);
            }

            // Subtrai o peso da matéria-prima
            if (novoProduto.Peso.HasValue) // Verifica se Peso tem um valor
            {
                materiaPrima.Peso -= novoProduto.Peso.Value; // Usando Value para acessar o valor
            }

            // Atualiza a matéria-prima
            _db.MateriaPrima.Update(materiaPrima);

            // Salva as mudanças no banco de dados
            _db.SaveChanges();

            // Redireciona para a página de produtos ou onde você desejar
            return RedirectToAction("Index");
        }

    }
}
