using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public class PastaPrincipalService : IPastaPrincipalService
    {
        private readonly VoxDocsContext _context;

        public PastaPrincipalService(VoxDocsContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DTOPastaPrincipal>> GetAllAsync()
        {
            var pastas = await _context.PastaPrincipal
                .Include(p => p.SubPastas) // Include SubPastas to count documents
                .ToListAsync();

            return pastas.Select(p => new DTOPastaPrincipal
            {
                Id = p.Id,
                NomePastaPrincipal = p.NomePastaPrincipal,
                EmpresaContratante = p.EmpresaContratante,
                Quantidade = p.SubPastas?.Count ?? 0 // Set Quantidade
            });
        }
        public async Task<DTOPastaPrincipal> GetByNamePrincipalAsync(string nomePasta)
        {
            var model = await _context.PastaPrincipal
                .FirstOrDefaultAsync(p => p.NomePastaPrincipal == nomePasta);
            return model is null 
                ? null 
                : new DTOPastaPrincipal {
                    Id = model.Id,
                    NomePastaPrincipal = model.NomePastaPrincipal,
                };
        }
        public async Task<IEnumerable<DTOPastaPrincipal>> GetByEmpresaAsync(string empresaContratante)
        {
            var pastas = await _context.PastaPrincipal
                .Include(p => p.SubPastas) // Inclui SubPastas para contar documentos
                .Where(p => p.EmpresaContratante == empresaContratante) // Filtra pela empresa
                .ToListAsync();

            return pastas.Select(p => new DTOPastaPrincipal
            {
                Id = p.Id,
                NomePastaPrincipal = p.NomePastaPrincipal,
                EmpresaContratante = p.EmpresaContratante,
                Quantidade = p.SubPastas?.Count ?? 0 // Define a quantidade de subpastas
            });
        }
        public async Task<DTOPastaPrincipal?> GetByIdAsync(int id)
        {
            var pasta = await _context.PastaPrincipal.FindAsync(id);
            if (pasta == null) return null;

            return new DTOPastaPrincipal
            {
                Id = pasta.Id,
                NomePastaPrincipal = pasta.NomePastaPrincipal,
                EmpresaContratante = pasta.EmpresaContratante
            };
        }
        public async Task<DTOPastaPrincipal> CreateAsync(DTOPastaPrincipalCreate dto)
        {
            var pasta = new PastaPrincipalModel
            {
                NomePastaPrincipal = dto.NomePastaPrincipal,
                EmpresaContratante = dto.EmpresaContratante
            };

            _context.PastaPrincipal.Add(pasta);
            await _context.SaveChangesAsync();

            return new DTOPastaPrincipal
            {
                Id = pasta.Id,
                NomePastaPrincipal = pasta.NomePastaPrincipal,
                EmpresaContratante = pasta.EmpresaContratante
            };
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var pasta = await _context.PastaPrincipal.FindAsync(id);
            if (pasta == null) return false;

            _context.PastaPrincipal.Remove(pasta);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}