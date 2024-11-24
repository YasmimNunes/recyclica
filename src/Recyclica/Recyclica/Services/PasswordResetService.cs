using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Recyclica.Data;
using Recyclica.Models;

namespace Recyclica.Services
{
    public class PasswordResetService
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método para gerar um código de 6 dígitos
        private string GenerateResetCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // Gera um código de 6 dígitos
        }

        // Método para criar e salvar o pedido de recuperação de senha
        public async Task<string> CreatePasswordResetRequestAsync(string userId)
        {
            var resetCode = GenerateResetCode();
            var expirationTime = DateTime.UtcNow.AddHours(24); // Código expira em 24 horas

            var passwordResetRequest = new PasswordResetRequest
            {
                UserId = userId,
                ResetCode = resetCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expirationTime,
                Status = "Pending"
            };

            _context.PasswordResetRequests.Add(passwordResetRequest);
            await _context.SaveChangesAsync();

            return resetCode;
        }
    }
}