using FluentValidation;
using VoxDocs.Models;
using VoxDocs.Data.Repositories;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Results;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace VoxDocs.Validators
{
    public interface ISubPastaValidator
    {
        Task<ValidationResult> ValidateCreate(SubPastaModel subPasta);
        Task<ValidationResult> ValidateUpdate(SubPastaModel subPasta);
        Task<ValidationResult> ValidateDelete(Guid id);
    }

    public class SubPastaValidator : AbstractValidator<SubPastaModel>, ISubPastaValidator
    {
        private readonly ISubPastaRepository _repository;

        public SubPastaValidator(ISubPastaRepository repository)
        {
            _repository = repository;

            RuleFor(x => x.NomeSubPasta)
                .NotEmpty().WithMessage("O nome da subpasta é obrigatório")
                .Length(3, 100).WithMessage("O nome da subpasta deve ter entre 3 e 100 caracteres");

            RuleFor(x => x.PastaPrincipalId)
                .NotEmpty().WithMessage("A pasta principal associada é obrigatória");
        }

        public async Task<ValidationResult> ValidateCreate(SubPastaModel subPasta)
        {
            var result = await this.ValidateAsync(subPasta);
            if (!result.IsValid) return result;
            return result;
        }

        public async Task<ValidationResult> ValidateUpdate(SubPastaModel subPasta)
        {
            var result = await this.ValidateAsync(subPasta);
            if (!result.IsValid) return result;

            var subPastaExistente = await _repository.ObterPorIdAsync(subPasta.Id);
            if (subPastaExistente == null)
            {
                result.Errors.Add(new ValidationFailure("Id", "Subpasta não encontrada."));
                return result;
            }
            return result;
        }

        public async Task<ValidationResult> ValidateDelete(Guid id)
        {
            var result = new ValidationResult();
            var subPasta = await _repository.ObterPorIdAsync(id);
            
            if (subPasta == null)
            {
                result.Errors.Add(new ValidationFailure("Id", "Subpasta não encontrada."));
            }

            return result;
        }
    }
}