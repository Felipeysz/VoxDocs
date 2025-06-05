using VoxDocs.DTO;

namespace VoxDocs.Services
{
    public interface IPagamentoConcluidoService
    {
        Task<PagamentoConcluidoDto> CriarPagamentoConcluidoAsync(PagamentoConcluidoCreateDto dto);
    }
}