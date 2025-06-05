using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public class SuporteVoxDocsService : ISuporteVoxDocsService
    {
        private readonly VoxDocsContext _context;

        public SuporteVoxDocsService(VoxDocsContext context)
        {
            _context = context;
        }

        public async Task<ChamadoModel> AbrirChamadoAsync(DTOAbrirChamado dto)
        {
            var chamado = new ChamadoModel
            {
                Assunto = dto.Assunto,
                Descricao = dto.Descricao,
                UsuarioId = dto.UsuarioId,
                NomeUsuario = dto.NomeUsuario,
                DataCriacao = DateTime.UtcNow,
                Status = StatusChamado.Aberto
            };

            _context.Chamados.Add(chamado);
            await _context.SaveChangesAsync();
            return chamado;
        }

        public async Task<IList<ChamadoModel>> ObterTodosChamadosAsync()
        {
            return await _context.Chamados
                .Include(c => c.Mensagens)
                .OrderByDescending(c => c.DataCriacao)
                .ToListAsync();
        }

        public async Task<IList<ChamadoModel>> ObterChamadosAbertosAsync()
        {
            return await _context.Chamados
                .Where(c => c.Status == StatusChamado.Aberto || c.Status == StatusChamado.EmAtendimento)
                .Include(c => c.Mensagens)
                .OrderByDescending(c => c.DataCriacao)
                .ToListAsync();
        }

        public async Task<ChamadoModel> ObterChamadoPorIdAsync(int chamadoId)
        {
            var chamado = await _context.Chamados
                .Include(c => c.Mensagens.OrderBy(m => m.DataEnvio))
                .FirstOrDefaultAsync(c => c.Id == chamadoId);

            if (chamado == null)
                throw new KeyNotFoundException($"Chamado com ID {chamadoId} não encontrado.");

            return chamado;
        }

        public async Task<MensagemModel> ResponderChamadoAsync(DTOResponderChamado dto)
        {
            // Verifica se o chamado existe
            var chamado = await _context.Chamados.FindAsync(dto.ChamadoId);
            if (chamado == null)
                throw new KeyNotFoundException($"Chamado com ID {dto.ChamadoId} não encontrado.");

            // Se for a primeira resposta do suporte, vamos atualizar o status p/ EmAtendimento
            if (chamado.Status == StatusChamado.Aberto)
            {
                chamado.Status = StatusChamado.EmAtendimento;
                chamado.SuporteResponsavelId = dto.SuporteResponsavelId;
                chamado.NomeSuporteResponsavel = dto.NomeSuporteResponsavel;
            }

            // Cria a mensagem de suporte
            var mensagem = new MensagemModel
            {
                ChamadoId = dto.ChamadoId,
                Conteudo = dto.Mensagem,
                DataEnvio = DateTime.UtcNow,
                EnviadoPorSuporte = true,
                NomeRemetente = dto.NomeSuporteResponsavel
            };

            _context.Mensagens.Add(mensagem);
            await _context.SaveChangesAsync();
            return mensagem;
        }

        public async Task<ChamadoModel> FinalizarChamadoAsync(DTOFinalizarChamado dto)
        {
            var chamado = await _context.Chamados
                .Include(c => c.Mensagens)
                .FirstOrDefaultAsync(c => c.Id == dto.ChamadoId);

            if (chamado == null)
                throw new KeyNotFoundException($"Chamado com ID {dto.ChamadoId} não encontrado.");

            // Verifica se é o mesmo responsável ou privilégio (por simplicidade, não checamos autorização aqui)
            chamado.Status = StatusChamado.Resolvido;
            chamado.DataFinalizacao = DateTime.UtcNow;
            chamado.SuporteResponsavelId = dto.SuporteResponsavelId;
            chamado.NomeSuporteResponsavel = dto.NomeSuporteResponsavel;

            // Se vier uma mensagem de encerramento, adiciona como última mensagem
            if (!string.IsNullOrWhiteSpace(dto.MensagemEncerramento))
            {
                var msgEnc = new MensagemModel
                {
                    ChamadoId = chamado.Id,
                    Conteudo = dto.MensagemEncerramento,
                    DataEnvio = DateTime.UtcNow,
                    EnviadoPorSuporte = true,
                    NomeRemetente = dto.NomeSuporteResponsavel
                };
                _context.Mensagens.Add(msgEnc);
            }

            await _context.SaveChangesAsync();
            return chamado;
        }

        public async Task<ChamadoModel> ArquivarChamadoAsync(int chamadoId)
        {
            var chamado = await _context.Chamados.FindAsync(chamadoId);
            if (chamado == null)
                throw new KeyNotFoundException($"Chamado com ID {chamadoId} não encontrado.");

            // Só arquiva se estiver resolvido ou fechado
            if (chamado.Status != StatusChamado.Resolvido && chamado.Status != StatusChamado.Fechado)
                throw new InvalidOperationException("Somente chamados resolvidos/fechados podem ser arquivados.");

            chamado.Status = StatusChamado.Arquivado;
            await _context.SaveChangesAsync();
            return chamado;
        }

        public async Task<ChamadoModel> ReabrirChamadoAsync(int chamadoId)
        {
            var chamado = await _context.Chamados.FindAsync(chamadoId);
            if (chamado == null)
                throw new KeyNotFoundException($"Chamado com ID {chamadoId} não encontrado.");

            // Só reabre se estiver arquivado
            if (chamado.Status != StatusChamado.Arquivado)
                throw new InvalidOperationException("Somente chamados arquivados podem ser reabertos.");

            chamado.Status = StatusChamado.Aberto;
            chamado.DataFinalizacao = null;
            await _context.SaveChangesAsync();
            return chamado;
        }

        public async Task<bool> DeletarChamadoAsync(int chamadoId)
        {
            var chamado = await _context.Chamados
                .Include(c => c.Mensagens)
                .FirstOrDefaultAsync(c => c.Id == chamadoId);

            if (chamado == null)
                return false;

            // Remove primeiro as mensagens filhas para não violar FK
            _context.Mensagens.RemoveRange(chamado.Mensagens);
            _context.Chamados.Remove(chamado);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
