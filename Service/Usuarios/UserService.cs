// Services/UserService.cs
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Helpers;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public class UserService : IUserService
    {
        private readonly VoxDocsContext _context;
        public UserService(VoxDocsContext context)
            => _context = context;

        public async Task<UserModel> RegisterUserAsync(DTOUser dto)
        {
            var user = new UserModel
            {
                Usuario = dto.Usuario,
                Email = dto.Email,
                Senha = PasswordHelper.HashPassword(dto.Senha),
                PermissionAccount = dto.PermissionAccount,
                EmpresaContratante = dto.EmpresaContratante,
                PlanoPago = dto.PlanoPago,
                LimiteUsuario = dto.LimiteUsuario,
                LimiteAdmin = dto.LimiteAdmin
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }


        public async Task<ClaimsPrincipal> ValidateUserAsync(DTOUserLogin dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Usuario == dto.Usuario);
            if (user == null)
                throw new KeyNotFoundException("Conta inexistente.");

            if (user.Senha != PasswordHelper.HashPassword(dto.Senha))
                throw new UnauthorizedAccessException("Senha incorreta.");

            // Criar ClaimsPrincipal sem tocar no banco
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Usuario),
                new Claim("PermissionAccount", user.PermissionAccount)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        public async Task<List<UserModel>> GetUsersAsync()
            => await _context.Users.ToListAsync();

        public async Task<UserModel> UpdateUserAsync(DTOUser dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Usuario == dto.Usuario)
                       ?? throw new KeyNotFoundException("Usuário não encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Senha))
                user.Senha = PasswordHelper.HashPassword(dto.Senha);
            if (!string.IsNullOrWhiteSpace(dto.PermissionAccount))
                user.PermissionAccount = dto.PermissionAccount;

            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id)
                       ?? throw new KeyNotFoundException("Usuário não encontrado.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<UserModel> GetUserByIdAsync(int id)
            => await _context.Users.FindAsync(id)
                ?? throw new KeyNotFoundException("Usuário não encontrado por ID.");

        public async Task<UserModel> GetUserByUsernameAsync(string username)
            => await _context.Users.FirstOrDefaultAsync(u => u.Usuario == username)
                ?? throw new KeyNotFoundException("Usuário não encontrado por nome.");
        public async Task<bool> UpdatePasswordAsync(string usuario, string senhaAtual, string novaSenha)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Usuario == usuario)
                    ?? throw new KeyNotFoundException("Usuário não encontrado.");

            if (user.Senha != PasswordHelper.HashPassword(senhaAtual))
                throw new UnauthorizedAccessException("Senha atual incorreta.");

            user.Senha = PasswordHelper.HashPassword(novaSenha);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<UserModel> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task SavePasswordResetTokenAsync(int userId, string token)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("Usuário não encontrado.");
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1); // expira em 1h
            await _context.SaveChangesAsync();
        }

        public async Task<int?> GetUserIdByResetTokenAsync(string token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiration > DateTime.UtcNow);
            return user?.Id;
        }

        public async Task UpdatePasswordByIdAsync(int userId, string novaSenha)
        {
            var user = await _context.Users.FindAsync(userId)
                ?? throw new KeyNotFoundException("Usuário não encontrado.");
            user.Senha = PasswordHelper.HashPassword(novaSenha);
            await _context.SaveChangesAsync();
        }

        public async Task InvalidatePasswordResetTokenAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
            if (user != null)
            {
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiration = null;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<DateTime?> GetPasswordResetTokenExpirationAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
            return user?.PasswordResetTokenExpiration;
        }
    }  
}
