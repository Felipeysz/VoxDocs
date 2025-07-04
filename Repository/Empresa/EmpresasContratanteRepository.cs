using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VoxDocs.Data;
using VoxDocs.Models;

namespace VoxDocs.Data.Repositories;

public interface IEmpresasContratanteRepository
{
    Task<IEnumerable<EmpresasContratanteModel>> ObterTodosAsync(bool incluirPastas = false, CancellationToken cancellationToken = default);
    Task<EmpresasContratanteModel?> ObterPorIdAsync(Guid id, bool incluirPastas = false, CancellationToken cancellationToken = default);
    Task<EmpresasContratanteModel?> ObterPorNomeAsync(string nomeEmpresa, bool incluirPastas = false, CancellationToken cancellationToken = default);
    Task<bool> ExistePorAsync(Expression<Func<EmpresasContratanteModel, bool>> predicado, CancellationToken cancellationToken = default);
    Task<EmpresasContratanteModel> AdicionarAsync(EmpresasContratanteModel empresa, CancellationToken cancellationToken = default);
    Task AtualizarAsync(EmpresasContratanteModel empresa, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
    IQueryable<EmpresasContratanteModel> Consulta(bool incluirPastas = false);
    Task<bool> ExisteAsync(Guid empresaId);
}

public class EmpresasContratanteRepository(VoxDocsContext context) : IEmpresasContratanteRepository
{
    private readonly VoxDocsContext _context = context;

    public async Task<IEnumerable<EmpresasContratanteModel>> ObterTodosAsync(bool incluirPastas = false, CancellationToken cancellationToken = default)
    {
        var query = _context.EmpresasContratantes.AsNoTracking();
        
        if (incluirPastas)
        {
            query = query.Include(e => e.PastasPrincipais);
        }

        return await query
            .OrderBy(e => e.EmpresaNome)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmpresasContratanteModel?> ObterPorIdAsync(Guid id, bool incluirPastas = false, CancellationToken cancellationToken = default)
    {
        var query = _context.EmpresasContratantes.AsNoTracking();
        
        if (incluirPastas)
        {
            query = query.Include(e => e.PastasPrincipais);
        }

        return await query
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EmpresasContratanteModel?> ObterPorNomeAsync(string nomeEmpresa, bool incluirPastas = false, CancellationToken cancellationToken = default)
    {
        var query = _context.EmpresasContratantes.AsNoTracking();
        
        if (incluirPastas)
        {
            query = query.Include(e => e.PastasPrincipais);
        }

        return await query
            .FirstOrDefaultAsync(e => e.EmpresaNome == nomeEmpresa, cancellationToken);
    }

    public async Task<bool> ExistePorAsync(Expression<Func<EmpresasContratanteModel, bool>> predicado, CancellationToken cancellationToken = default)
    {
        return await _context.EmpresasContratantes
            .AnyAsync(predicado, cancellationToken);
    }

    public async Task<EmpresasContratanteModel> AdicionarAsync(EmpresasContratanteModel empresa, CancellationToken cancellationToken = default)
    {
        empresa.Id = empresa.Id == Guid.Empty ? Guid.NewGuid() : empresa.Id;
        
        await _context.EmpresasContratantes.AddAsync(empresa, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return empresa;
    }
    
    public async Task AtualizarAsync(EmpresasContratanteModel empresa, CancellationToken cancellationToken = default)
    {
        _context.Entry(empresa).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var empresa = await _context.EmpresasContratantes
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Empresa com ID {id} não encontrada.");

        _context.EmpresasContratantes.Remove(empresa);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<EmpresasContratanteModel> Consulta(bool incluirPastas = false)
    {
        var query = _context.EmpresasContratantes.AsQueryable();
        
        if (incluirPastas)
        {
            query = query.Include(e => e.PastasPrincipais);
        }

        return query;
    }

    public async Task<bool> ExisteAsync(Guid empresaId)
    {
        return await _context.EmpresasContratantes
            .AnyAsync(e => e.Id == empresaId);
    }
}