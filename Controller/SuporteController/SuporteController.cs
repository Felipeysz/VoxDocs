using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using VoxDocs.Models;

namespace VoxDocs.ControllersMvc
{
    [Authorize]
    public class SuporteController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private const string CacheKey = "SuporteTickets";

        public SuporteController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;

            // Inicializa a cache vazia
            if (!_memoryCache.TryGetValue(CacheKey, out List<SuporteDetailsViewModel> _))
            {
                _memoryCache.Set(CacheKey, new List<SuporteDetailsViewModel>());
            }
        }

        private List<SuporteDetailsViewModel> GetTicketsFromCache()
        {
            return _memoryCache.Get<List<SuporteDetailsViewModel>>(CacheKey) ?? new List<SuporteDetailsViewModel>();
        }

        private void SaveTicketsToCache(List<SuporteDetailsViewModel> tickets)
        {
            _memoryCache.Set(CacheKey, tickets);
        }

        // GET: Suporte/Lista
        public IActionResult SuporteList()
        {
            var tickets = GetTicketsFromCache();

            var model = new SuporteListViewModel
            {
                ChamadosAbertos = tickets.Where(c => c.Status == "Aberto" || c.Status == "Em Atendimento").ToList(),
                ChamadosFechados = tickets.Where(c => c.Status == "Resolvido" || c.Status == "Fechado")
                                        .OrderByDescending(c => c.DataResolucao)
                                        .Take(5)
                                        .ToList()
            };

            return View(model);
        }

        // GET: Suporte/DetailsView/{id}
        [Authorize(Roles = "SuporteVoxDocsAdmin,SuporteVoxDocs")]
        public IActionResult SuporteDetailsView(int id)
        {
            var chamado = GetTicketsFromCache().FirstOrDefault(c => c.Id == id);
            if (chamado == null) return NotFound();

            return View(chamado);
        }

        // GET: Suporte/DetailsActive/{id}
        [Authorize(Roles = "SuporteVoxDocsAdmin,SuporteVoxDocs")]
        public IActionResult SuporteDetailsActive(int id)
        {
            var tickets = GetTicketsFromCache();
            var chamado = tickets.FirstOrDefault(c => c.Id == id);
            if (chamado == null) return NotFound();

            // Garante que está em atendimento
            chamado.Status = "Em Atendimento";
            if (string.IsNullOrEmpty(chamado.Responsavel))
            {
                chamado.Responsavel = User.Identity?.Name ?? "Atendente";
            }

            SaveTicketsToCache(tickets);
            return View(chamado);
        }

        // GET: Suporte/DetailsResolved/{id}
        [Authorize(Roles = "SuporteVoxDocsAdmin,SuporteVoxDocs")]
        public IActionResult SuporteDetailsResolved(int id)
        {
            var tickets = GetTicketsFromCache();
            var chamado = tickets.FirstOrDefault(c => c.Id == id);
            if (chamado == null) return NotFound();

            // Garante que está resolvido
            chamado.Status = "Resolvido";
            if (!chamado.DataResolucao.HasValue)
            {
                chamado.DataResolucao = DateTime.Now;
            }

            SaveTicketsToCache(tickets);
            return View(chamado);
        }

        // POST: Suporte/SubmitSupportTicket
        [HttpPost]
        public IActionResult SubmitSupportTicket(string problemType, string problemDetails)
        {
            var tickets = GetTicketsFromCache();
            var nextId = tickets.Any() ? tickets.Max(t => t.Id) + 1 : 1;

            var novoChamado = new SuporteDetailsViewModel
            {
                Id = nextId,
                Assunto = GetAssuntoFromProblemType(problemType),
                Status = "Aberto",
                DataCriacao = DateTime.Now,
                Responsavel = null,
                Mensagens = new List<MensagemSuporte>
                {
                    new MensagemSuporte
                    {
                        Conteudo = problemDetails,
                        DataEnvio = DateTime.Now,
                        Origem = "Cliente",
                        Remetente = User.Identity?.Name ?? "Usuário"
                    }
                },
                CriadoPor = User.Identity?.Name ?? "Usuário"
            };

            tickets.Add(novoChamado);
            SaveTicketsToCache(tickets);

            return RedirectToAction(nameof(SuporteList));
        }

        private string GetAssuntoFromProblemType(string problemType)
        {
            return problemType switch
            {
                "login" => "Problema com login",
                "upload" => "Documentos não carregam",
                "sync" => "Problemas de sincronização",
                "share" => "Dificuldade para compartilhar",
                "format" => "Formatação incorreta",
                "search" => "Busca não funciona",
                "security" => "Problema de segurança",
                _ => "Outro problema"
            };
        }

        // Ações para manipular os chamados
        [HttpPost]
        [Authorize(Roles = "SuporteVoxDocsAdmin,SuporteVoxDocs")]
        public IActionResult AtenderChamado(int id)
        {
            var tickets = GetTicketsFromCache();
            var chamado = tickets.FirstOrDefault(c => c.Id == id);
            if (chamado == null) return NotFound();

            chamado.Status = "Em Atendimento";
            chamado.Responsavel = User.Identity?.Name ?? "Atendente";

            SaveTicketsToCache(tickets);
            return RedirectToAction(nameof(SuporteDetailsActive), new { id });
        }

        [HttpPost]
        [Authorize(Roles = "SuporteVoxDocsAdmin,SuporteVoxDocs")]
        public IActionResult FecharChamado(int id)
        {
            var tickets = GetTicketsFromCache();
            var chamado = tickets.FirstOrDefault(c => c.Id == id);
            if (chamado == null) return NotFound();

            chamado.Status = "Fechado";
            chamado.DataResolucao = DateTime.Now;

            SaveTicketsToCache(tickets);
            return RedirectToAction(nameof(SuporteDetailsResolved), new { id });
        }

        [HttpPost]
        public IActionResult AumentarPrioridade(int id)
        {
            return RedirectToAction(nameof(SuporteList));
        }

        [HttpPost]
        [Authorize(Roles = "SuporteVoxDocsAdmin,SuporteVoxDocs")]
        public IActionResult ArquivarChamado(int id)
        {
            var tickets = GetTicketsFromCache();
            var chamado = tickets.FirstOrDefault(c => c.Id == id);
            if (chamado != null)
            {
                tickets.Remove(chamado);
                SaveTicketsToCache(tickets);
            }

            return RedirectToAction(nameof(SuporteList));
        }

        // GET: Suporte/OpenUser
        public IActionResult SuporteOpenUser()
        {
            var tickets = GetTicketsFromCache();
            var nextId = tickets.Any() ? tickets.Max(t => t.Id) + 1 : 1;

            var novoChamado = new SuporteDetailsViewModel
            {
                Id = nextId,
                Assunto = "Novo Chamado",
                Status = "Aberto",
                DataCriacao = DateTime.Now,
                Responsavel = null,
                Mensagens = new List<MensagemSuporte>(),
                CriadoPor = User.Identity?.Name ?? "Usuário"
            };

            tickets.Add(novoChamado);
            SaveTicketsToCache(tickets);

            return View(novoChamado);
        }

        // GET: Suporte/VerificarAtualizacao/{id}
        public IActionResult VerificarAtualizacao(int id)
        {
            var chamado = GetTicketsFromCache().FirstOrDefault(c => c.Id == id);
            if (chamado == null) return NotFound();

            return Json(new { atualizado = false });
        }
    }
}