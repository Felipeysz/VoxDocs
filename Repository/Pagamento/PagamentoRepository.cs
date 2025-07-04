using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using VoxDocs.Data;
using VoxDocs.Models;

namespace VoxDocs.Repositories
{
    public interface IPagamentoAtivoRepository
    {
        IExecutionStrategy GetExecutionStrategy();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<IEnumerable<PagamentoAtivoModel>> ObterTodosAsync(CancellationToken cancellationToken = default);
        Task<PagamentoAtivoModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagamentoAtivoModel?> ObterPorEmpresaAsync(string empresaNome, CancellationToken cancellationToken = default);
        Task<bool> ExistePorAsync(Expression<Func<PagamentoAtivoModel, bool>> predicado, CancellationToken cancellationToken = default);
        Task<PagamentoAtivoModel> AdicionarAsync(PagamentoAtivoModel pagamento, CancellationToken cancellationToken = default);
        Task AtualizarAsync(PagamentoAtivoModel pagamento, CancellationToken cancellationToken = default);
        Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
        IQueryable<PagamentoAtivoModel> Consulta();
        Task<IEnumerable<PagamentoAtivoModel>> ObterExpiradosAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<PagamentoAtivoModel>> ObterAtivosAsync(CancellationToken cancellationToken = default);
    }

    public class PagamentoAtivoRepository : IPagamentoAtivoRepository
    {
        private readonly VoxDocsContext _context;

        public PagamentoAtivoRepository(VoxDocsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

            public IExecutionStrategy GetExecutionStrategy()
            {
                return _context.Database.CreateExecutionStrategy();
            }
            
            public async Task<IDbContextTransaction> BeginTransactionAsync()
            {
                return await _context.Database.BeginTransactionAsync();
            }
    

        public async Task<IEnumerable<PagamentoAtivoModel>> ObterTodosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Pagamento
                .AsNoTracking()
                .OrderBy(p => p.EmpresaNome)
                .ToListAsync(cancellationToken);
        }

        public async Task<PagamentoAtivoModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Pagamento
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<PagamentoAtivoModel?> ObterPorEmpresaAsync(string empresaNome, CancellationToken cancellationToken = default)
        {
            return await _context.Pagamento
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.EmpresaNome == empresaNome, cancellationToken);
        }

        public async Task<bool> ExistePorAsync(Expression<Func<PagamentoAtivoModel, bool>> predicado, CancellationToken cancellationToken = default)
        {
            return await _context.Pagamento
                .AnyAsync(predicado, cancellationToken);
        }

        public async Task<PagamentoAtivoModel> AdicionarAsync(PagamentoAtivoModel pagamento, CancellationToken cancellationToken = default)
        {
            pagamento.Id = pagamento.Id == Guid.Empty ? Guid.NewGuid() : pagamento.Id;
            
            await _context.Pagamento.AddAsync(pagamento, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return pagamento;
        }

        public async Task AtualizarAsync(PagamentoAtivoModel pagamento, CancellationToken cancellationToken = default)
        {
            _context.Entry(pagamento).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var pagamento = await _context.Pagamento
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Pagamento com ID {id} não encontrado.");

            _context.Pagamento.Remove(pagamento);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public IQueryable<PagamentoAtivoModel> Consulta()
        {
            return _context.Pagamento.AsQueryable();
        }

        public async Task<IEnumerable<PagamentoAtivoModel>> ObterExpiradosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Pagamento
                .AsNoTracking()
                .Where(p => p.DataExpiracao < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<PagamentoAtivoModel>> ObterAtivosAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Pagamento
                .AsNoTracking()
                .Where(p => p.DataExpiracao >= DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }
    }
}