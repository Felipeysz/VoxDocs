using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.Models;

namespace VoxDocs.Validators
{
    public class EmpresasContratanteBusinessValidator : AbstractValidator<EmpresasContratanteModel>
    {
        private readonly VoxDocsContext _context;

        public EmpresasContratanteBusinessValidator(VoxDocsContext context)
        {
            _context = context;

            RuleFor(x => x.EmpresaNome)
                .NotEmpty().WithMessage("O nome da empresa é obrigatório.")
                .MaximumLength(100).WithMessage("O nome não pode exceder 100 caracteres.");

            RuleFor(x => x.EmailEmpresa)
                .EmailAddress().WithMessage("Email em formato inválido.")
                .MaximumLength(100).WithMessage("O email não pode exceder 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.EmailEmpresa));

            RuleFor(x => x.PlanoId)
                .NotEmpty().WithMessage("O plano é obrigatório.");

            // Regras de negócio
            RuleFor(x => x.EmpresaNome)
                .MustAsync(async (model, nome, cancellation) => 
                    !await ExisteNomeEmpresa(nome, model.Id))
                .WithMessage("Já existe uma empresa com este nome.");

            RuleFor(x => x.EmailEmpresa)
                .MustAsync(async (model, email, cancellation) => 
                    string.IsNullOrEmpty(email) || !await ExisteEmailEmpresa(email, model.Id))
                .WithMessage("Já existe uma empresa com este email.");
        }

        private async Task<bool> ExisteNomeEmpresa(string nome, Guid id)
        {
            return await _context.EmpresasContratantes
                .AnyAsync(e => e.EmpresaNome == nome && e.Id != id);
        }

        private async Task<bool> ExisteEmailEmpresa(string email, Guid id)
        {
            return await _context.EmpresasContratantes
                .AnyAsync(e => e.EmailEmpresa == email && e.Id != id);
        }
    }
}