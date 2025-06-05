using System.Collections.Generic;
using System.Threading.Tasks;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public interface IPlanosVoxDocsService
    {
        Task<List<PlanosVoxDocsModel>> GetAllPlansAsync();
        Task<List<PlanosVoxDocsModel>> GetPlansByCategoryAsync(string categoria);
        Task<PlanosVoxDocsModel> GetPlanByIdAsync(int id);
        Task<PlanosVoxDocsModel> CreatePlanAsync(DTOPlanosVoxDocs dto);
        Task<PlanosVoxDocsModel> UpdatePlanAsync(int id, DTOPlanosVoxDocs dto);
         Task<PlanosVoxDocsModel> GetPlanByNameAsync(string name);
        Task DeletePlanAsync(int id);
    }
}
