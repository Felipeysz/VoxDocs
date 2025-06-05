using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxDocs.DTO;
using VoxDocs.Models;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    [Authorize]
    public class SuporteVoxDocsMvcController : Controller
    {
        private readonly ISuporteVoxDocsService _suporteService;

        public SuporteVoxDocsMvcController(ISuporteVoxDocsService suporteService)
        {
            _suporteService = suporteService;
        }

        /// GET /SuporteVoxDocsMvc/Listar
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var todos = await _suporteService.ObterTodosChamadosAsync();

            var abertos = todos
                .Where(c => c.Status == StatusChamado.Aberto
                         || c.Status == StatusChamado.EmAtendimento)
                .OrderByDescending(c => c.DataCriacao)
                .ToList();

            var fechados = todos
                .Where(c => c.Status == StatusChamado.Resolvido
                         || c.Status == StatusChamado.Fechado
                         || c.Status == StatusChamado.Arquivado)
                .OrderByDescending(c => c.DataFinalizacao ?? c.DataCriacao)
                .ToList();

            ViewBag.ChamadosAbertos = abertos;
            ViewBag.ChamadosFechados = fechados;

            return View("~/Views/SuporteVoxDocs/SuporteListar.cshtml");
        }

        /// GET /SuporteVoxDocsMvc/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ChamadoModel chamado;
            try
            {
                chamado = await _suporteService.ObterChamadoPorIdAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Chamado de ID {id} não existe.");
            }

            if (chamado.Status == StatusChamado.Resolvido
             || chamado.Status == StatusChamado.Fechado
             || chamado.Status == StatusChamado.Arquivado)
            {
                return View(
                    "~/Views/SuporteVoxDocs/SuporteResolved.cshtml",
                    chamado);
            }

            if (chamado.Status == StatusChamado.Aberto
             && chamado.SuporteResponsavelId == null)
            {
                return View(
                    "~/Views/SuporteVoxDocs/SuporteVisualizar.cshtml",
                    chamado);
            }

            return View(
                "~/Views/SuporteVoxDocs/SuporteAtivo.cshtml",
                chamado);
        }

        /// GET /SuporteVoxDocsMvc/AbrirChamadoSuporte
        [HttpGet]
        public IActionResult AbrirChamadoSuporte()
        {
            // Preenche o model com dados básicos da conta do usuário logado.
            // Exemplo (você deve ajustar para recuperar o ID e o nome reais do usuário logado):
            var usuarioId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var nomeUsuario = User.Identity?.Name ?? "";

            var dto = new DTOAbrirChamado
            {
                UsuarioId = usuarioId,
                NomeUsuario = nomeUsuario
            };
            return View("~/Views/Pages/AbrirChamadoSuporte.cshtml", dto);
        }

        /// <summary>
        /// POST /SuporteVoxDocsMvc/AbrirChamadoSuporte
        /// Recebe dados do formulário e cria o novo chamado.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AbrirChamadoSuporte(DTOAbrirChamado dto)
        {
            if (!ModelState.IsValid)
            {
                // Retorna a mesma view, exibindo erros de validação
                return View("~/Views/Pages/AbrirChamadoSuporte.cshtml", dto);
            }

            try
            {
                await _suporteService.AbrirChamadoAsync(dto);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("~/Views/Pages/AbrirChamadoSuporte.cshtml", dto);
            }

            // Após criar, redireciona para a lista de chamados
            return RedirectToAction(nameof(Listar));
        }

        /// <summary>
        /// POST /SuporteVoxDocsMvc/Responder
        /// Adiciona nova mensagem de suporte e, se for o primeiro atendimento, muda status para EmAtendimento.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(int id, string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
            {
                TempData["ErroMensagem"] = "A mensagem não pode ficar vazia.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var suporteId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var nomeSuporte = User.Identity?.Name ?? "Suporte";

            var dto = new DTOResponderChamado
            {
                ChamadoId = id,
                SuporteResponsavelId = suporteId,
                NomeSuporteResponsavel = nomeSuporte,
                Mensagem = mensagem
            };

            try
            {
                await _suporteService.ResponderChamadoAsync(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Chamado de ID {id} não encontrado.");
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /SuporteVoxDocsMvc/Finalizar
        /// Finaliza (resolve) um chamado e adiciona mensagem de encerramento opcional.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id, string mensagemEncerramento)
        {
            var suporteId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var nomeSuporte = User.Identity?.Name ?? "Suporte";

            var dto = new DTOFinalizarChamado
            {
                ChamadoId = id,
                SuporteResponsavelId = suporteId,
                MensagemEncerramento = mensagemEncerramento
            };

            try
            {
                await _suporteService.FinalizarChamadoAsync(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Chamado de ID {id} não encontrado.");
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// POST /SuporteVoxDocsMvc/Arquivar
        /// Arquiva um chamado que esteja “Resolvido” ou “Fechado”.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Arquivar(int id)
        {
            try
            {
                await _suporteService.ArquivarChamadoAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Chamado de ID {id} não encontrado.");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErroArquivar"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }

            return RedirectToAction(nameof(Listar));
        }
    }
}
