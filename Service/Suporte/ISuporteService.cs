using System.Collections.Generic;
using System.Threading.Tasks;
using VoxDocs.DTO;
using VoxDocs.Models;

namespace VoxDocs.Services
{
    public interface ISuporteVoxDocsService
    {
        // 1) Abrir um chamado
        Task<ChamadoModel> AbrirChamadoAsync(DTOAbrirChamado dto);

        // 2) Listar todos os chamados (poderíamos filtrar por status, mas aqui retornaremos tudo)
        Task<IList<ChamadoModel>> ObterTodosChamadosAsync();

        // 3) Listar chamados abertos (status = Aberto)
        Task<IList<ChamadoModel>> ObterChamadosAbertosAsync();

        // 4) Obter um chamado específico por id (com todas as mensagens)
        Task<ChamadoModel> ObterChamadoPorIdAsync(int chamadoId);

        // 5) Suporte responde um chamado (adiciona mensagem e atualiza status p/ EmAtendimento)
        Task<MensagemModel> ResponderChamadoAsync(DTOResponderChamado dto);

        // 6) Finalizar (resolver) um chamado (Status -> Resolvido)
        Task<ChamadoModel> FinalizarChamadoAsync(DTOFinalizarChamado dto);

        // 7) Arquivar o chamado (Mover para histórico) — status -> Arquivado
        Task<ChamadoModel> ArquivarChamadoAsync(int chamadoId);

        // 8) Voltar um chamado de Arquivado para Aberto (caso precise reabrir)
        Task<ChamadoModel> ReabrirChamadoAsync(int chamadoId);

        // 9) Deletar um chamado (caso deseje remover completamente)
        Task<bool> DeletarChamadoAsync(int chamadoId);
    }
}
