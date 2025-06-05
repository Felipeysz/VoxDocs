using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Models;


namespace VoxDocs.Services
{
    public class SubPastaService : ISubPastaService
    {
        private readonly VoxDocsContext _context;

        public SubPastaService(VoxDocsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DTOSubPasta>> GetAllAsync()
        {
            var subPastas = await _context.SubPastas
                .Include(sp => sp.Documentos) // Include Documentos to count documents
                .ToListAsync();

            return subPastas.Select(sp => new DTOSubPasta
            {
                Id = sp.Id,
                NomeSubPasta = sp.NomeSubPasta,
                NomePastaPrincipal = sp.NomePastaPrincipal,
                EmpresaContratante = sp.EmpresaContratante,
                Quantidade = sp.Documentos?.Count ?? 0 // Set Quantidade
            });
        }
        
        public async Task<IEnumerable<DTOSubPasta>> GetByEmpresaAsync(string empresa)
        {
            return await _context.SubPastas
                .Where(s => s.EmpresaContratante == empresa)
                .Select(s => new DTOSubPasta
                {
                    Id = s.Id,
                    NomeSubPasta = s.NomeSubPasta,
                    EmpresaContratante = s.EmpresaContratante
                })
                .ToListAsync();
        }

            public async Task<DTOSubPasta> GetByNameSubPastaAsync(string nomeSubPasta)
        {
            var model = await _context.SubPastas
                .FirstOrDefaultAsync(s => s.NomeSubPasta == nomeSubPasta);
            return model is null
                ? null
                : new DTOSubPasta
                {
                    Id = model.Id,
                    NomeSubPasta = model.NomeSubPasta,
                    // … demais campos
                };
        }


        public async Task<DTOSubPasta?> GetByIdAsync(int id)
        {
            var subPasta = await _context.SubPastas.FindAsync(id);
            if (subPasta == null) return null;

            return new DTOSubPasta
            {
                Id = subPasta.Id,
                NomeSubPasta = subPasta.NomeSubPasta,
                NomePastaPrincipal = subPasta.NomePastaPrincipal,
                EmpresaContratante = subPasta.EmpresaContratante
            };
        }

        public async Task<DTOSubPasta> CreateAsync(DTOSubPastaCreate dto)
        {
            var subPasta = new SubPastaModel
            {
                NomeSubPasta = dto.NomeSubPasta,
                NomePastaPrincipal = dto.NomePastaPrincipal,
                EmpresaContratante = dto.EmpresaContratante
            };

            _context.SubPastas.Add(subPasta);
            await _context.SaveChangesAsync();

            return new DTOSubPasta
            {
                Id = subPasta.Id,
                NomeSubPasta = subPasta.NomeSubPasta,
                NomePastaPrincipal = subPasta.NomePastaPrincipal,
                EmpresaContratante = subPasta.EmpresaContratante
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var subPasta = await _context.SubPastas.FindAsync(id);
            if (subPasta == null) return false;

            _context.SubPastas.Remove(subPasta);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DTOSubPasta>> GetSubChildrenAsync(string nomePastaPrincipal)
            {
                var subPastas = await _context.SubPastas
                    .Where(sp => sp.NomePastaPrincipal.Trim().ToLower() == nomePastaPrincipal.Trim().ToLower())
                    .ToListAsync();

                return subPastas.Select(sp => new DTOSubPasta
                {
                    Id = sp.Id,
                    NomeSubPasta = sp.NomeSubPasta,
                    NomePastaPrincipal = sp.NomePastaPrincipal,
                    EmpresaContratante = sp.EmpresaContratante
                });
            }
    }
}