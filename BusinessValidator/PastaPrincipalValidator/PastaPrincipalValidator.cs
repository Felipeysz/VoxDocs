using FluentValidation;
using FluentValidation.Results;
using VoxDocs.Models;

namespace VoxDocs.Validators
{
    public interface IPastaPrincipalValidator
    {
        ValidationResult Validate(PastaPrincipalModel pasta);
    }

    public class PastaPrincipalValidator : AbstractValidator<PastaPrincipalModel>, IPastaPrincipalValidator
    {
        public PastaPrincipalValidator()
        {
            // Regras básicas de validação
            RuleFor(pasta => pasta.NomePastaPrincipal)
                .NotEmpty().WithMessage("O nome da pasta principal é obrigatório.")
                .MaximumLength(100).WithMessage("O nome da pasta principal não pode exceder 100 caracteres.");

            RuleFor(pasta => pasta.EmpresaId)
                .NotEmpty().WithMessage("A empresa associada é obrigatória.");
        }

        public new ValidationResult Validate(PastaPrincipalModel model)
        {
            return base.Validate(model);
        }
    }
}