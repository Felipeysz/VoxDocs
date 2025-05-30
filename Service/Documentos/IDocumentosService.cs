using System.Collections.Generic;
using System.Threading.Tasks;
using VoxDocs.DTO;

namespace VoxDocs.Services
{
    public interface IDocumentoService
    {
        Task<bool> ValidateTokenDocumentoAsync(string nomeArquivo, string token);
        Task<DTODocumentoCreate> GetByIdAsync(int Id, string? token = null);
        Task DeleteAsync(int Id, string? token);
        Task<IEnumerable<DTODocumentoCreate>> GetAllAsync();
        Task<IEnumerable<DTODocumentoCreate>> GetBySubPastaAsync(string subPasta);
        Task<IEnumerable<DTODocumentoCreate>> GetByPastaPrincipalAsync(string pastaPrincipal);
        Task<DTODocumentoCreate> CreateAsync(DocumentoDto dto);
        Task<DTODocumentoCreate> UpdateAsync(DocumentoUpdateDto dto);
        Task<DTOQuantidadeDocumentoEmpresa> GetEstatisticasEmpresaAsync(string empresa);
        Task<DTOAcessosDocumento> GetAcessosDocumentoAsync(int Id, int dias);
        Task IncrementarAcessoAsync(int Id);
        Task<bool> ArquivoExisteAsync(string nomeArquivo);
        Task<(Stream stream, string contentType)> DownloadDocumentoProtegidoAsync(string nomeArquivo, string token = null);
    }
}
