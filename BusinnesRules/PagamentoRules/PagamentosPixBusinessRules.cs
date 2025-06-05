using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.Models;

namespace VoxDocs.PagamentosBusinessRules
{
    public class PagamentosPixBusinessRules
    {
        private readonly VoxDocsContext _context;

        public PagamentosPixBusinessRules(VoxDocsContext context)
        {
            _context = context;
        }

        public async Task ValidarPagamentoEmpresaAsync(string empresaContratante, string tipoPlano)
        {
            // Verificar se já existe pagamento ativo para esta empresa e plano
            var pagamentoAtivo = await _context.PagamentosPix
                .Where(p => p.EmpresaContratante == empresaContratante &&
                            p.TipoPlano == tipoPlano &&
                            p.DataExpiracao > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (pagamentoAtivo != null)
            {
                throw new InvalidOperationException(
                    $"Já existe um pagamento ativo para este plano válido até {pagamentoAtivo.DataExpiracao:dd/MM/yyyy}");
            }
        }
    }
}