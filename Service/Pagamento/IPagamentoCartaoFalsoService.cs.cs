// IPagamentoCartaoFalsoService.cs
using VoxDocs.DTO;
using System.Threading.Tasks;

namespace VoxDocs.Services
{
    public interface IPagamentoCartaoFalsoService
    {
        Task<string> ProcessarPagamentoCartaoFalsoAsync(PagamentoCartaoRequestDto dto);
    }
}
