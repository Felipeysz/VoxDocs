using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoxDocs.Data;
using VoxDocs.Models;

namespace VoxDocs.Services.Validators
{
    public interface IDocumentoValidator
    {
        Task ValidateCreateAsync(DocumentoModel documento, CancellationToken cancellationToken = default);
        Task ValidateUpdateAsync(DocumentoModel documento, CancellationToken cancellationToken = default);
    }

    public class DocumentoValidator : AbstractValidator<DocumentoModel>, IDocumentoValidator
    {
        private readonly VoxDocsContext _context;
        private readonly ILogger<DocumentosService> _logger;

            public DocumentoValidator()
            {
                RuleFor(x => x.NomeArquivo)
                    .NotEmpty().WithMessage("O nome do arquivo é obrigatório")
                    .MaximumLength(255).WithMessage("O nome do arquivo não pode exceder 255 caracteres");

                RuleFor(x => x.DescricaoArquivo)
                    .MaximumLength(500).WithMessage("A descrição não pode exceder 500 caracteres");

                RuleFor(x => x.IdPastaPrincipal)
                    .NotEmpty().WithMessage("A pasta principal é obrigatória");

                RuleFor(x => x.NivelPrioridade)
                    .InclusiveBetween(1, 3).WithMessage("Nível de prioridade inválido");

                RuleFor(x => x.TokenAcesso)
                    .NotEmpty().When(x => x.NivelPrioridade == 3)
                    .WithMessage("Token de acesso é obrigatório para documentos confidenciais");
            }

        public async Task ValidateCreateAsync(DocumentoModel documento, CancellationToken cancellationToken = default)
        {
            var result = await this.ValidateAsync(documento, cancellationToken);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        public async Task ValidateUpdateAsync(DocumentoModel documento, CancellationToken cancellationToken = default)
        {
            var result = await this.ValidateAsync(documento, cancellationToken);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }

        private async Task<bool> BeUniqueNameInFolder(DocumentoModel documento, string nomeArquivo, CancellationToken cancellationToken)
        {
            return !await _context.Documentos.AnyAsync(d =>
                d.NomeArquivo == nomeArquivo &&
                d.IdPastaPrincipal == documento.IdPastaPrincipal &&
                d.IdSubPasta == documento.IdSubPasta &&
                d.Id != documento.Id,
                cancellationToken);
        }

        private async Task<bool> ExistEmpresa(Guid empresaId, CancellationToken cancellationToken)
        {
            // Adicione logging para depuração
            _logger.LogDebug($"Verificando existência da empresa com ID: {empresaId}");

            try
            {
                var existe = await _context.EmpresasContratantes
                    .AnyAsync(e => e.Id == empresaId, cancellationToken);

                _logger.LogDebug($"Empresa {empresaId} existe: {existe}");
                return existe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao verificar empresa {empresaId}");
                throw;
            }
        }

        private async Task<bool> ExistPastaPrincipal(Guid pastaId, CancellationToken cancellationToken)
        {
            return await _context.PastasPrincipais.AnyAsync(p => p.Id == pastaId, cancellationToken);
        }

        private async Task<bool> ExistSubPastaWhenNotNull(Guid? subPastaId, CancellationToken cancellationToken)
        {
            if (!subPastaId.HasValue) return true;
            return await _context.SubPastas.AnyAsync(s => s.Id == subPastaId.Value, cancellationToken);
        }
    }
}