using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Data.Repositories
{
    public interface IPastaPrincipalRepository
    {
        Task<PastaPrincipalModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<PastaPrincipalModel>> ObterPorEmpresaIdAsync(Guid empresaId, bool incluirSubPastas = false, CancellationToken cancellationToken = default);
        Task<PastaPrincipalModel> AdicionarAsync(PastaPrincipalModel pasta, CancellationToken cancellationToken = default);
        Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PastaPrincipalModel?> ObterPorNomeEEmpresaAsync(string nome, Guid empresaId, CancellationToken cancellationToken = default);
    }

    public class PastaPrincipalRepository : IPastaPrincipalRepository
    {
        private readonly VoxDocsContext _context;

        public PastaPrincipalRepository(VoxDocsContext context)
        {
            _context = context;
        }

        // Implementação no PastaPrincipalRepository
        public async Task<PastaPrincipalModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.PastasPrincipais
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<PastaPrincipalModel>> ObterPorEmpresaIdAsync(Guid empresaId, bool incluirSubPastas = false, CancellationToken cancellationToken = default)
        {
            var query = _context.PastasPrincipais.AsNoTracking()
                .Where(p => p.EmpresaId == empresaId);

            if (incluirSubPastas)
            {
                query = query.Include(p => p.SubPastas);
            }

            return await query
                .OrderBy(p => p.NomePastaPrincipal)
                .ToListAsync(cancellationToken);
        }
        public async Task<PastaPrincipalModel> AdicionarAsync(PastaPrincipalModel pasta, CancellationToken cancellationToken = default)
        {
            pasta.Id = pasta.Id == Guid.Empty ? Guid.NewGuid() : pasta.Id;

            await _context.PastasPrincipais.AddAsync(pasta, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return pasta;
        }
        public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var pasta = await _context.PastasPrincipais
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Pasta principal com ID {id} não encontrada.");

            _context.PastasPrincipais.Remove(pasta);
            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<PastaPrincipalModel?> ObterPorNomeEEmpresaAsync(string nome, Guid empresaId, CancellationToken cancellationToken = default)
        {
            return await _context.PastasPrincipais
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.NomePastaPrincipal == nome &&
                    p.EmpresaId == empresaId,
                    cancellationToken);
        }

        public async Task<bool> ExistePorAsync(Expression<Func<EmpresasContratanteModel, bool>> predicado, CancellationToken cancellationToken = default)
        {
            return await _context.EmpresasContratantes
                .AnyAsync(predicado, cancellationToken);
        }
    }
}