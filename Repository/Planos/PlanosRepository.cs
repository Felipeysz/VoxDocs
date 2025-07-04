using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VoxDocs.Models;

namespace VoxDocs.Data.Repositories
{
    public interface IPlanosVoxDocsRepository
    {
        Task<IEnumerable<PlanosVoxDocsModel>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<PlanosVoxDocsModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PlanosVoxDocsModel?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default);
        Task<PlanosVoxDocsModel?> ObterPorNomeEPeriodicidadeAsync(string nome, string periodicidade, CancellationToken cancellationToken = default);
        Task<IEnumerable<PlanosVoxDocsModel>> ObterPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default);
        Task<bool> ExistePorAsync(Expression<Func<PlanosVoxDocsModel, bool>> predicado, CancellationToken cancellationToken = default);
        Task<PlanosVoxDocsModel> AdicionarAsync(PlanosVoxDocsModel plano, CancellationToken cancellationToken = default);
        Task AtualizarAsync(PlanosVoxDocsModel plano, CancellationToken cancellationToken = default);
        Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
        IQueryable<PlanosVoxDocsModel> Consulta();
    }

    public class PlanosVoxDocsRepository(VoxDocsContext context) : IPlanosVoxDocsRepository
    {
        private readonly VoxDocsContext _context = context;

        public async Task<IEnumerable<PlanosVoxDocsModel>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.PlanosVoxDocs
                .AsNoTracking()
                .OrderBy(p => p.Nome)
                .ToListAsync(cancellationToken);
        }

        public async Task<PlanosVoxDocsModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.PlanosVoxDocs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<PlanosVoxDocsModel?> ObterPorNomeAsync(string nome, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(nome))
                return null;

            return await _context.PlanosVoxDocs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Nome == nome, cancellationToken);
        }

        public async Task<PlanosVoxDocsModel?> ObterPorNomeEPeriodicidadeAsync(string nome, string periodicidade, CancellationToken cancellationToken = default)
        {
            return await _context.PlanosVoxDocs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => 
                    p.Nome.Trim().ToLower() == nome.Trim().ToLower() &&
                    p.Periodicidade.Trim().ToLower() == periodicidade.Trim().ToLower(), 
                    cancellationToken);
        }

        public async Task<IEnumerable<PlanosVoxDocsModel>> ObterPorCategoriaAsync(string categoria, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                return Enumerable.Empty<PlanosVoxDocsModel>();

            var pattern = $"%{categoria.Trim()}%";
            return await _context.PlanosVoxDocs
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.Nome, pattern))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistePorAsync(Expression<Func<PlanosVoxDocsModel, bool>> predicado, CancellationToken cancellationToken = default)
        {
            return await _context.PlanosVoxDocs
                .AnyAsync(predicado, cancellationToken);
        }

        public async Task<PlanosVoxDocsModel> AdicionarAsync(PlanosVoxDocsModel plano, CancellationToken cancellationToken = default)
        {
            plano.Id = plano.Id == Guid.Empty ? Guid.NewGuid() : plano.Id;
            
            await _context.PlanosVoxDocs.AddAsync(plano, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return plano;
        }

        public async Task AtualizarAsync(PlanosVoxDocsModel plano, CancellationToken cancellationToken = default)
        {
            _context.Entry(plano).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var plano = await _context.PlanosVoxDocs
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Plano com ID {id} não encontrado.");

            _context.PlanosVoxDocs.Remove(plano);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<PlanosVoxDocsModel> Consulta() => 
            _context.PlanosVoxDocs.AsQueryable();
    }
}