using System.Collections.Generic;
using System.Threading.Tasks;
using VoxDocs.DTO;

namespace VoxDocs.Services
{
    public interface ISubPastaService
    {
        Task<IEnumerable<DTOSubPasta>> GetAllAsync();
        Task<DTOSubPasta?> GetByIdAsync(int id);
        Task<DTOSubPasta> CreateAsync(DTOSubPastaCreate dto);
        Task<bool> DeleteAsync(int id);
        Task<DTOSubPasta> GetByNameSubPastaAsync(string nomeSubPasta);
        Task<IEnumerable<DTOSubPasta>> GetSubChildrenAsync(string nomePastaPrincipal);
        Task<IEnumerable<DTOSubPasta>> GetByEmpresaAsync(string empresa);

    }
}