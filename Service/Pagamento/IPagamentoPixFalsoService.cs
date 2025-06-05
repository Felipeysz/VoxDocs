// IPagamentoPixFalsoService.cs
using VoxDocs.DTO;
using System.Threading.Tasks;

namespace VoxDocs.Services
{
    public interface IPagamentoPixFalsoService
    {
        Task<(int pagamentoPixId, string qrCodeUrl)> GerarPixAsync(PagamentoPixRequestDto dto);
        Task<bool> TokenPixExisteAsync(string token); // Novo método
    }
}