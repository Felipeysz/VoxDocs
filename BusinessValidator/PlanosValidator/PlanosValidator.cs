using FluentValidation;
using VoxDocs.Models;

namespace VoxDocs.BusinessRules
{
    public interface IPlanosValidator : IValidator<PlanosVoxDocsModel>
    {
        // Métodos específicos para validação de negócio podem ser declarados aqui
        bool ValidarDuracaoPorPlano(string nomePlano, int? duracao);
        bool ValidarLimitesPorPlano(string nomePlano, int? limiteAdmin, int? limiteUsuario);
    }

    public class PlanosValidator : AbstractValidator<PlanosVoxDocsModel>, IPlanosValidator
    {
        private static readonly string[] PeriodicidadesValidas = { "Mensal", "Semestral", "Anual", "Ilimitado" };
        private static readonly string[] PlanosPermitidos = { "Gratuito", "Premium", "Premium Semestral", "Premium Anual" };

        public PlanosValidator()
        {
            // removendo as validações básicas que já existem na model
            RuleFor(p => p.Nome)
                .Must(nome => PlanosPermitidos.Contains(nome, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Plano inválido. Os planos permitidos são: {string.Join(", ", PlanosPermitidos)}.");

            // Validação de negócio específica para descrição (tamanho mínimo)
            RuleFor(p => p.Descriçao)
                .MinimumLength(10).WithMessage("A descrição deve ter pelo menos 10 caracteres.");

            // Validação de periodicidade customizada
            RuleFor(p => p.Periodicidade)
                .Must(p => PeriodicidadesValidas.Contains(p))
                .WithMessage($"Periodicidade inválida. Use: {string.Join(", ", PeriodicidadesValidas)}.");

            // Validações de negócio específicas
            RuleFor(p => p.Duracao)
                .Must((plano, duracao) => ValidarDuracaoPorPlano(plano.Nome, duracao))
                .WithMessage("A duração não é válida para este tipo de plano.");

            RuleFor(p => p.ArmazenamentoDisponivel)
                .LessThanOrEqualTo(1024).WithMessage("O armazenamento não pode exceder 1024 GB (1TB).");

            RuleFor(p => p.LimiteAdmin)
                .Must((plano, limite) => ValidarLimitesPorPlano(plano.Nome, limite, plano.LimiteUsuario))
                .WithMessage("Os limites não são válidos para este tipo de plano.");
        }

        public bool ValidarDuracaoPorPlano(string nomePlano, int? duracao)
        {
            return nomePlano switch
            {
                "Gratuito" => duracao == 0,
                "Premium" => new[] { 1 }.Contains(duracao.Value),
                "Premium Semestral" => new[] { 6 }.Contains(duracao.Value),
                "Premium Anual" => new[] { 12 }.Contains(duracao.Value),
                _ => false
            };
        }

        public bool ValidarLimitesPorPlano(string nomePlano, int? limiteAdmin, int? limiteUsuario)
        {
            if (limiteAdmin < -2 || limiteUsuario < -2) return false;

            return nomePlano switch
            {
                "Gratuito" => limiteAdmin <= 2 && limiteUsuario <= 5,
                "Premium" or "Premium Semestral" or "Premium Anual" => limiteAdmin == -1 && limiteUsuario == -1,
                _ => false
            };
        }
    }
}