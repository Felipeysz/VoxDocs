using FluentValidation;
using FluentValidation.Results;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Validators
{
    public interface IPagamentoValidator : IValidator<PagamentoDTO>
    {
        bool BeAValidDate(string date);
        bool BeFutureDate(string date);
        ValidationResult ValidateRegistroNovoPagamento(PagamentoAtivoModel pagamento);
        ValidationResult ValidatePagamentoAtivo(PagamentoAtivoModel pagamento);
        ValidationResult ValidateRenovacaoPagamento(PagamentoAtivoModel pagamento);
    }

    public class PagamentoValidator : AbstractValidator<PagamentoDTO>, IPagamentoValidator
    {
        public PagamentoValidator()
        {
            RuleFor(p => p.PlanoId)
                .NotEmpty().WithMessage("Plano é obrigatório");
        }

        public bool BeAValidDate(string date)
        {
            return DateTime.TryParseExact(date, "MM/yy", null, System.Globalization.DateTimeStyles.None, out _);
        }

        public bool BeFutureDate(string date)
        {
            if (DateTime.TryParseExact(date, "MM/yy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                var lastDayOfMonth = new DateTime(parsedDate.Year, parsedDate.Month, 
                    DateTime.DaysInMonth(parsedDate.Year, parsedDate.Month));
                return lastDayOfMonth > DateTime.Now;
            }
            return false;
        }

        public ValidationResult ValidateRegistroNovoPagamento(PagamentoAtivoModel pagamento)
        {
            var validator = new PagamentoAtivoValidator();
            return validator.Validate(pagamento);
        }

        public ValidationResult ValidatePagamentoAtivo(PagamentoAtivoModel pagamento)
        {
            var validator = new PagamentoAtivoValidator();
            return validator.Validate(pagamento, options => 
            {
                options.IncludeRuleSets("Ativo");
            });
        }

        public ValidationResult ValidateRenovacaoPagamento(PagamentoAtivoModel pagamento)
        {
            var validator = new PagamentoAtivoValidator();
            return validator.Validate(pagamento, options => 
            {
                options.IncludeRuleSets("Renovacao");
            });
        }
    }

    public class PagamentoAtivoValidator : AbstractValidator<PagamentoAtivoModel>
    {
        public PagamentoAtivoValidator()
        {
            // Regras básicas para todos os cenários
            RuleFor(p => p.EmpresaNome)
                .NotEmpty().WithMessage("Nome da empresa é obrigatório")
                .MaximumLength(100).WithMessage("Nome da empresa não pode exceder 100 caracteres");

            RuleFor(p => p.NomePlanoPago)
                .NotEmpty().WithMessage("Nome do plano é obrigatório");

            RuleFor(p => p.DataPagamento)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de pagamento não pode ser futura");

            // Regras específicas para registro novo
            RuleSet("Novo", () => 
            {
                RuleFor(p => p.DataExpiracao)
                    .GreaterThan(DateTime.UtcNow).WithMessage("Data de expiração deve ser futura");
            });

            // Regras específicas para pagamento ativo
            RuleSet("Ativo", () => 
            {
                RuleFor(p => p.DataExpiracao)
                    .GreaterThan(DateTime.UtcNow).WithMessage("Pagamento não está mais ativo");
            });

            // Regras específicas para renovação
            RuleSet("Renovacao", () => 
            {
                RuleFor(p => p.DataExpiracao)
                    .GreaterThan(DateTime.UtcNow.AddMonths(1)).WithMessage("Nova data de expiração deve ser pelo menos 1 mês no futuro");
            });
        }
    }
}