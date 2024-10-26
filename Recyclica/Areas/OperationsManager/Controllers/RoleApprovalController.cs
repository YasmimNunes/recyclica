using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recyclica.Data;
using Recyclica.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recyclica.Areas.OperationsManager.Controllers
{
    [Area("OperationsManager")]
    public class RoleApprovalController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public RoleApprovalController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Obter aprovações que ainda não foram aprovadas ou estão pendentes (Approved == null ou false)
            var pendingApprovals = await _dbContext.UserRolesApproval
                .Include(u => u.User)
                .Include(r => r.Role)
                .Where(x => x.Approved == false || x.Approved == null)  // Apenas não aprovados ou pendentes
                .Select(x => new UserRolesApprovalViewModel
                {
                    Id = x.Id,
                    UserName = x.User != null ? x.User.UserName ?? "Unknown User" : "Unknown User",
                    RoleName = x.Role != null ? x.Role.Name ?? "Unknown Role" : "Unknown Role",
                    Approved = x.Approved,
                    ApprovedBy = x.ApprovedBy ?? "-",
                    ApprovalDate = x.ApprovalDate.HasValue ? x.ApprovalDate.Value.ToString("dd/MM/yyyy") : "-"
                })
                .ToListAsync();

            // Filtrar aprovações com Approved == true para a tabela de usuários aprovados
            var approvedUserIds = await _dbContext.UserRolesApproval
                .Where(a => a.Approved == true)  // Apenas os aprovados
                .Select(a => a.UserId)
                .Distinct()
                .ToListAsync();

            // Obter a lista de usuários cujos IDs estão aprovados
            var users = await _userManager.Users
                .Where(user => approvedUserIds.Contains(user.Id))  // Filtra usuários aprovados
                .Select(user => new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    LockoutEnabled = user.LockoutEnabled
                })
                .ToListAsync();

            // Para cada usuário, buscar o cargo (role) separadamente
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(await _userManager.FindByIdAsync(user.Id));
                user.RoleName = roles.FirstOrDefault() ?? "Sem cargo";  // Pega o primeiro cargo ou define "Sem cargo"
            }

            var viewModel = new RoleApprovalAndUsersViewModel
            {
                RoleApprovals = pendingApprovals,  // Apenas aprovações pendentes
                Users = users  // Apenas os usuários aprovados
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Editar(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var fullName = _dbContext.Entry(user).Property<string>("FullName").CurrentValue;

            var approval = await _dbContext.UserRolesApproval
                .Where(a => a.UserId == id && a.Approved == true)
                .Select(a => new
                {
                    a.ApprovalDate
                })
                .FirstOrDefaultAsync();

            var viewModel = new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = fullName, 
                Email = user.Email,
                RoleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "Sem cargo",
                PhoneNumber = user.PhoneNumber,
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Editar(UserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            user.PhoneNumber = model.PhoneNumber;
            _dbContext.Entry(user).Property<string>("FullName").CurrentValue = model.FullName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Erro ao atualizar o usuário.");
                return View(model);
            }

            await _dbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dados do usuário atualizados com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            // Busca o registro na tabela UserRolesApproval
            var approval = await _dbContext.UserRolesApproval
                .FirstOrDefaultAsync(a => a.Id == id);

            if (approval == null)
            {
                return NotFound(); // Retorna 404 se o registro não for encontrado
            }

            approval.Approved = true; // Marca como aprovado
            approval.ApprovedBy = User.Identity.Name ?? "Unknown"; // Atribui o nome do usuário logado
            approval.ApprovalDate = DateTime.Now; // Atribui a data atual

            await _dbContext.SaveChangesAsync(); // Salva as alterações

            return RedirectToAction(nameof(Index)); // Redireciona para o índice
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            // Busca o registro na tabela UserRolesApproval
            var approval = await _dbContext.UserRolesApproval
                .Include(u => u.User) // Inclui o relacionamento com o usuário
                .FirstOrDefaultAsync(a => a.Id == id);

            if (approval == null)
            {
                return NotFound(); // Retorna 404 se o registro não for encontrado
            }

            // Deleta o usuário da tabela AspNetUsers
            var userId = approval.UserId; // Captura o ID do usuário a ser excluído

            // Remove da tabela UserRolesApproval
            _dbContext.UserRolesApproval.Remove(approval);

            // Remove o usuário da tabela AspNetUsers
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _userManager.DeleteAsync(user); // Deleta o usuário
            }

            await _dbContext.SaveChangesAsync(); // Salva as alterações

            return RedirectToAction(nameof(Index)); // Redireciona para o índice
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(); 
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Erro ao excluir o usuário.");
            }

            // Remover registros de aprovação relacionados ao usuário
            var approvals = await _dbContext.UserRolesApproval
                .Where(a => a.UserId == id)
                .ToListAsync();

            if (approvals.Any())
            {
                _dbContext.UserRolesApproval.RemoveRange(approvals);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index)); 
        }
    }

    // ViewModel para os usuários
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }  
        public bool EmailConfirmed { get; set; }
        public string PhoneNumber { get; set; }
        public bool LockoutEnabled { get; set; }
        public string ApprovalDate { get; set; }
        public string FullName { get; set; }
    }

    // ViewModel que combina aprovações e usuários
    public class RoleApprovalAndUsersViewModel
    {
        public List<UserRolesApprovalViewModel> RoleApprovals { get; set; }
        public List<UserViewModel> Users { get; set; }
    }

}
