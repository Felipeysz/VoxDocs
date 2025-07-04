using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.Models;

namespace VoxDocs.Data.Repositories
{
    public interface IDocumentoRepository
    {
        Task<DocumentoModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentoModel>> ObterPorSubPastaAsync(Guid subPastaId, CancellationToken cancellationToken = default);
        Task<IEnumerable<DocumentoModel>> ObterPorPastaPrincipalAsync(Guid pastaPrincipalId, CancellationToken cancellationToken = default);
        Task<DocumentoModel> AdicionarAsync(DocumentoModel documento, CancellationToken cancellationToken = default);
        Task<DocumentoModel> AtualizarAsync(DocumentoModel documento, CancellationToken cancellationToken = default);
        Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
    }

    public class DocumentoRepository : IDocumentoRepository
    {
        private readonly VoxDocsContext _context;

        public DocumentoRepository(VoxDocsContext context)
        {
            _context = context;
        }

        public async Task<DocumentoModel?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Documentos
                .Include(d => d.Empresa)
                .Include(d => d.PastaPrincipal)
                .Include(d => d.SubPasta)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<DocumentoModel>> ObterPorPastaPrincipalAsync(Guid pastaPrincipalId, CancellationToken cancellationToken = default)
        {
            return await _context.Documentos
                .AsNoTracking()
                .Where(d => d.IdPastaPrincipal == pastaPrincipalId && d.IdSubPasta == null)
                .Include(d => d.Empresa)
                .Include(d => d.PastaPrincipal)
                .OrderBy(d => d.NomeArquivo)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<DocumentoModel>> ObterPorSubPastaAsync(Guid subPastaId, CancellationToken cancellationToken = default)
        {
            return await _context.Documentos
                .AsNoTracking()
                .Where(d => d.IdSubPasta == subPastaId)
                .Include(d => d.Empresa)
                .Include(d => d.PastaPrincipal)
                .Include(d => d.SubPasta)
                .OrderBy(d => d.NomeArquivo)
                .ToListAsync(cancellationToken);
        }

        public async Task<DocumentoModel> AdicionarAsync(DocumentoModel documento, CancellationToken cancellationToken = default)
        {
            documento.Id = documento.Id == Guid.Empty ? Guid.NewGuid() : documento.Id;
            documento.DataCriacao = DateTime.UtcNow;

            await _context.Documentos.AddAsync(documento, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return documento;
        }

        public async Task<DocumentoModel> AtualizarAsync(DocumentoModel documento, CancellationToken cancellationToken = default)
        {
            documento.DataAlteracao = DateTime.UtcNow;
            _context.Entry(documento).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);
            return documento;
        }

        public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var documento = await _context.Documentos
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
                ?? throw new KeyNotFoundException($"Documento com ID {id} não encontrado.");

            _context.Documentos.Remove(documento);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}