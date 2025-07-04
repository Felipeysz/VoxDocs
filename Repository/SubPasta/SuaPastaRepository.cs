using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VoxDocs.Models;

namespace VoxDocs.Data.Repositories;

public interface ISubPastaRepository
{
    Task<SubPastaModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<SubPastaModel>> ObterPorPastaPrincipalIdAsync(Guid pastaPrincipalId, CancellationToken cancellationToken = default);
    Task<SubPastaModel> AdicionarAsync(SubPastaModel subPasta, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}

public class SubPastaRepository(VoxDocsContext context) : ISubPastaRepository
{
    private readonly VoxDocsContext _context = context;
    public async Task<SubPastaModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SubPastas
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken);
    }
    public async Task<IEnumerable<SubPastaModel>> ObterPorPastaPrincipalIdAsync(Guid pastaPrincipalId, CancellationToken cancellationToken = default)
    {
        return await _context.SubPastas
            .AsNoTracking()
            .Where(sp => sp.PastaPrincipalId == pastaPrincipalId)
            .OrderBy(sp => sp.NomeSubPasta)
            .ToListAsync(cancellationToken);
    }
    public async Task<SubPastaModel> AdicionarAsync(SubPastaModel subPasta, CancellationToken cancellationToken = default)
    {
        subPasta.Id = subPasta.Id == Guid.Empty ? Guid.NewGuid() : subPasta.Id;

        await _context.SubPastas.AddAsync(subPasta, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return subPasta;
    }
    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subPasta = await _context.SubPastas
            .FirstOrDefaultAsync(sp => sp.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Subpasta com ID {id} não encontrada.");

        _context.SubPastas.Remove(subPasta);
        await _context.SaveChangesAsync(cancellationToken);
    }
}