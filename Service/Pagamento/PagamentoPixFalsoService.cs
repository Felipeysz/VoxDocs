using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Models;
using VoxDocs.PagamentosBusinessRules;

namespace VoxDocs.Services
{
    public class PagamentoPixFalsoService : IPagamentoPixFalsoService
    {
        private readonly VoxDocsContext _context;
        private readonly PagamentosPixBusinessRules _pixRules;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IPagamentoConcluidoService _pagamentoConcluidoService;

        public PagamentoPixFalsoService(
            VoxDocsContext context,
            PagamentosPixBusinessRules pixRules,
            IPlanosVoxDocsService planosService,
            IPagamentoConcluidoService pagamentoConcluidoService)
        {
            _context = context;
            _pixRules = pixRules;
            _planosService = planosService;
            _pagamentoConcluidoService = pagamentoConcluidoService;
        }
        
        public async Task<bool> TokenPixExisteAsync(string token)
        {
            return await _context.PagamentosPix.AnyAsync(p => p.QRCodePix == token);
        }

        public async Task<(int pagamentoPixId, string mensagem, string qrCode)> GerarPixAsync(PagamentoPixRequestDto dto)
        {
            await _pixRules.ValidarPagamentoEmpresaAsync(dto.EmpresaContratante, dto.TipoPlano);

            var cutoff = DateTime.UtcNow.AddHours(-1);
            var antigos = await _context.PagamentosPix
                .Where(p => p.DataCriacao < cutoff)
                .ToListAsync();

            _context.PagamentosPix.RemoveRange(antigos);
            await _context.SaveChangesAsync();

            var plano = await _planosService.GetPlanByNameAsync(dto.TipoPlano)
                ?? throw new Exception("Plano não encontrado.");

            var token = Guid.NewGuid().ToString();
            var now = DateTime.UtcNow;

            var pix = new PagamentoPixModel
            {
                QRCodePix = token,
                TipoPlano = dto.TipoPlano,
                EmpresaContratante = dto.EmpresaContratante,
                DataCriacao = now
            };

            _context.PagamentosPix.Add(pix);
            await _context.SaveChangesAsync();

            await _pagamentoConcluidoService.CriarPagamentoConcluidoAsync(
                new PagamentoConcluidoCreateDto
                {
                    EmpresaContratante = dto.EmpresaContratante,
                    MetodoPagamento = "Pix",
                    DataPagamento = now,
                    DataExpiracao = now.AddMonths(plano.Duration)
                });

            return (pix.Id, "Pix gerado com sucesso", $"/ConfirmandoPagamento?token={token}");
        }

        Task<(int pagamentoPixId, string qrCodeUrl)> IPagamentoPixFalsoService.GerarPixAsync(PagamentoPixRequestDto dto)
        {
            throw new NotImplementedException();
        }

        Task<bool> IPagamentoPixFalsoService.TokenPixExisteAsync(string token)
        {
            throw new NotImplementedException();
        }
    }
}