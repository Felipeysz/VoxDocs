using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Models;
using VoxDocs.Services;

public class PagamentoConcluidoService : IPagamentoConcluidoService
{
    private readonly VoxDocsContext _context;

    public PagamentoConcluidoService(VoxDocsContext context)
    {
        _context = context;
    }

    public async Task<PagamentoConcluidoDto> CriarPagamentoConcluidoAsync(PagamentoConcluidoCreateDto dto)
    {
        var pagamento = new PagamentoConcluido
        {
            EmpresaContratante = dto.EmpresaContratante,
            MetodoPagamento = dto.MetodoPagamento,
            DataPagamento = dto.DataPagamento,
            DataExpiracao = dto.DataExpiracao,
            StatusEmpresa = "Plano Ativo"
        };

        _context.PagamentosConcluidos.Add(pagamento);
        await _context.SaveChangesAsync();
        
        return new PagamentoConcluidoDto
        {
            Id = pagamento.Id,
            EmpresaContratante = pagamento.EmpresaContratante,
            MetodoPagamento = pagamento.MetodoPagamento,
            DataPagamento = pagamento.DataPagamento,
            DataExpiracao = pagamento.DataExpiracao,
            StatusEmpresa = pagamento.StatusEmpresa
        };
    }
}