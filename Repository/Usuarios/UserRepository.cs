using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VoxDocs.Models;
using VoxDocs.Services;

namespace VoxDocs.Data.Repositories
{
    public interface IUserRepository
    {
        Task<UserModel> CreateAsync(UserModel user);
        Task<UserModel> GetByIdAsync(Guid id);
        Task<UserModel> GetByUsernameAsync(string username);
        Task<UserModel> GetByEmailAsync(string email);
        Task<UserModel> GetByEmailOrUsernameAsync(string email, string username);
        Task<UserModel> GetByResetTokenAsync(string token);
        Task<IEnumerable<UserModel>> GetAllAsync();
        Task<IEnumerable<UserModel>> FindAsync(Expression<Func<UserModel, bool>> predicate);
        Task<UserModel> UpdateAsync(UserModel user);
        Task DeleteAsync(Guid id);
        Task SetPasswordResetTokenAsync(Guid userId, string token, DateTime expiration);
        Task<bool> VerifyCredentialsAsync(string usernameOrEmail, string password);
        Task<bool> ExistsAsync(Expression<Func<UserModel, bool>> predicate);
    }

    public class UserRepository : IUserRepository
    {
        private readonly VoxDocsContext _context;
        private readonly ILogger<UserAuthService> _logger;

        public UserRepository(VoxDocsContext context,
            ILogger<UserAuthService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<UserModel> CreateAsync(UserModel user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // Validação redundante (segurança extra)
            if (string.IsNullOrWhiteSpace(user.Senha))
            {
                _logger.LogError("Tentativa de criar usuário com senha nula");
                throw new ArgumentException("Senha não pode ser nula", nameof(user.Senha));
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuário criado com ID: {UserId}", user.Id);
            return user;
        }

        public async Task<UserModel> GetByIdAsync(Guid id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<UserModel> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Usuario == username);
        }

        public async Task<UserModel> GetByEmailAsync(string email)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<UserModel> GetByEmailOrUsernameAsync(string email, string username)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email || u.Usuario == username);
        }

        public async Task<UserModel> GetByResetTokenAsync(string token)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.PasswordResetToken == token &&
                                      u.PasswordResetTokenExpiration > DateTime.UtcNow);
        }

        public async Task<IEnumerable<UserModel>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<UserModel>> FindAsync(Expression<Func<UserModel, bool>> predicate)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<UserModel, bool>> predicate)
        {
            return await _context.Users
                .AsNoTracking()
                .AnyAsync(predicate);
        }

        public async Task<UserModel> UpdateAsync(UserModel user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetPasswordResetTokenAsync(Guid userId, string token, DateTime expiration)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiration = expiration;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> VerifyCredentialsAsync(string usernameOrEmail, string password)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Usuario == usernameOrEmail || u.Email == usernameOrEmail);

            if (user == null) return false;

            return password == user.Senha;
        }
    }
}