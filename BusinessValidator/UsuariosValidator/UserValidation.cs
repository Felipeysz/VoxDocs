using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.Models;

namespace VoxDocs.BusinessRules
{
    public class UserValidation : AbstractValidator<UserModel>
    {
        private readonly VoxDocsContext _context;

        public UserValidation(VoxDocsContext context)
        {
            _context = context;


            //Validações para os limites
            RuleFor(u => u.LimiteUsuario)
                .Matches(@"^\d+$").WithMessage("O limite de usuários deve ser um número válido")
                .When(u => !string.IsNullOrEmpty(u.LimiteUsuario));

            RuleFor(u => u.LimiteAdmin)
                .Matches(@"^\d+$").WithMessage("O limite de administradores deve ser um número válido")
                .When(u => !string.IsNullOrEmpty(u.LimiteAdmin));

            // Validações complementares que não existem na model
            RuleFor(u => u.Usuario)
                .MustAsync(BeUniqueUsername).WithMessage("Já existe um usuário com este nome de usuário.")
                .Matches(@"^[a-zA-Z0-9_\-]+$").WithMessage("O nome de usuário só pode conter letras, números, hífens e underscores.");

            RuleFor(u => u.Email)
                .MustAsync(BeUniqueEmail).WithMessage("Já existe um usuário com este e-mail.");

            RuleFor(u => u.Senha)
                .Matches("[A-Z]").WithMessage("A senha deve conter pelo menos uma letra maiúscula.")
                .Matches("[0-9]").WithMessage("A senha deve conter pelo menos um número.")
                .Matches("[^a-zA-Z0-9]").WithMessage("A senha deve conter pelo menos um caractere especial.");

            // Validações de negócio que não são adequadas para Data Annotations
            RuleFor(u => u.PermissionAccount)
                .Must(p => p == "Admin" || p == "User" || p == "SuporteVoxDocs" || p == "SuporteVoxDocsAdmin")
                .WithMessage("Nível de permissão inválido. Valores aceitos: Admin, User, SuporteVoxDocs, SuporteVoxDocsAdmin");

            // Validação de formato específico para empresa contratante
            RuleFor(u => u.EmpresaContratante)
                .Matches(@"^[a-zA-Z0-9\s\.\-]+$")
                .WithMessage("Nome da empresa contém caracteres inválidos");
        }

        private async Task<bool> BeUniqueUsername(UserModel user, string username, CancellationToken cancellationToken)
        {
            return !await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Usuario == username && u.Id != user.Id, cancellationToken);
        }

        private async Task<bool> BeUniqueEmail(UserModel user, string email, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(email)) return true;
            
            return !await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Email == email && u.Id != user.Id, cancellationToken);
        }
    }
}