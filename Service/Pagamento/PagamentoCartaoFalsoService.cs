using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Models;
using VoxDocs.PagamentosBusinessRules;

namespace VoxDocs.Services
{
    public class PagamentoCartaoFalsoService : IPagamentoCartaoFalsoService
    {
        private readonly VoxDocsContext _context;
        private readonly PagamentosPixBusinessRules _pixRules;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IPagamentoConcluidoService _pagamentoConcluidoService;

        public PagamentoCartaoFalsoService(
            VoxDocsContext context,
            PagamentosPixBusinessRules pixRules,
            IPlanosVoxDocsService planosService,
            IPagamentoConcluidoService pagamentoConcluidoService)
        {
            _context = context;
            _pixRules = pixRules;
            _planosService = planosService;
            _pagamentoConcluidoService = pagamentoConcluidoService;
        }

        public async Task<string> ProcessarPagamentoCartaoFalsoAsync(PagamentoCartaoRequestDto dto)
        {
            await _pixRules.ValidarPagamentoEmpresaAsync(dto.EmpresaContratante, dto.TipoPlano);

            var plano = await _planosService.GetPlanByNameAsync(dto.TipoPlano)
                ?? throw new Exception("Plano não encontrado.");

            var pagamento = new PagamentoCartaoFalsoModel
            {
                CartaoNumber = dto.CartaoNumber,
                ValidadeCartao = dto.ValidadeCartao,
                CvvCartao = dto.CvvCartao,
                TipoCartao = dto.TipoCartao,
                TipoPlano = dto.TipoPlano,
                EmpresaContratante = dto.EmpresaContratante
            };

            _context.PagamentosCartao.Add(pagamento);
            await _context.SaveChangesAsync();

            await _pagamentoConcluidoService.CriarPagamentoConcluidoAsync(
                new PagamentoConcluidoCreateDto
                {
                    EmpresaContratante = dto.EmpresaContratante,
                    MetodoPagamento = "Cartão",
                    DataPagamento = DateTime.UtcNow,
                    DataExpiracao = DateTime.UtcNow.AddMonths(plano.Duration)
                });

            return "Pagamento com cartão processado com sucesso!";
        }
    }
}