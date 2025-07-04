using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using VoxDocs.DTO;
using VoxDocs.PagamentoViewModel;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    public partial class PagamentoController : Controller
    {
        private readonly IPagamentoService _pagamentoService;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IUserAuthService _userAuthService;
        private readonly ILogger<PagamentoController> _logger;
        private readonly IWebHostEnvironment _env;

        public PagamentoController(
            IPagamentoService pagamentoService,
            IPlanosVoxDocsService planosService,
            ILogger<PagamentoController> logger,
            IUserAuthService userAuthService,
            IWebHostEnvironment env)
        {
            _pagamentoService = pagamentoService;
            _planosService = planosService;
            _userAuthService = userAuthService;
            _logger = logger;
            _env = env;
        }

        [HttpGet]
        public IActionResult FinalizarPagamento()
        {
            if (!HttpContext.Session.HasEmpresa())
            {
                _logger.LogWarning("Tentativa de acessar FinalizarPagamento sem empresa na sessão");
                return RedirectToAction("CadastroEmpresa");
            }

            // Log dos dados da sessão para debug
            _logger.LogDebug("Dados da sessão ao finalizar pagamento: " +
                $"PlanoId={HttpContext.Session.GetPlanoId()}, " +
                $"EmpresaNome={HttpContext.Session.GetEmpresaNome()}, " +
                $"EmailEmpresa={HttpContext.Session.GetEmpresaEmail()}, " +
                $"UsuariosCount={HttpContext.Session.GetUsuarios()?.Count ?? 0}, " +
                $"PastasCount={HttpContext.Session.GetPastasPrincipais()?.Count ?? 0}");

            return View(new FinalizarPagamentoViewModel
            {
                PlanoId = HttpContext.Session.GetPlanoId(),
                NomePlano = HttpContext.Session.GetPlanoNome(),
                ValorPago = HttpContext.Session.GetPlanoPreco(),
                EmpresaNome = HttpContext.Session.GetEmpresaNome(),
                EmailEmpresa = HttpContext.Session.GetEmpresaEmail(),
                Usuarios = HttpContext.Session.GetUsuarios() ?? new List<PagamentoUsuarioDTO>(),
                Pastas = HttpContext.Session.GetPastasPrincipais()?.Select(p => p.Nome).ToList() ?? new List<string>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarPagamento(FinalizarPagamentoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var pagamentoDto = new PagamentoDTO
                {
                    PlanoId = HttpContext.Session.GetPlanoId(),
                    EmpresaNome = HttpContext.Session.GetEmpresaNome(),
                    EmailEmpresa = HttpContext.Session.GetEmpresaEmail(),
                    MetodoPagamento = model.MetodoPagamento,
                    PastasPrincipais = HttpContext.Session.GetPastasPrincipais(),
                    SubPastas = HttpContext.Session.GetSubPastas(),
                    Usuarios = HttpContext.Session.GetUsuarios() ?? new List<PagamentoUsuarioDTO>()
                };

                // Verificação adicional
                if (pagamentoDto.PastasPrincipais == null || !pagamentoDto.PastasPrincipais.Any())
                {
                    ModelState.AddModelError("", "Pelo menos uma pasta principal deve ser configurada.");
                    return View(model);
                }

                var resultado = await _pagamentoService.PagamentoConcluidoPlanoAsync(pagamentoDto);
                HttpContext.Session.Clear();

                return RedirectToAction("PagamentoConcluido", new
                {
                    empresaId = resultado.EmpresaId,
                    pagamentoId = resultado.PagamentoId,
                    planoId = resultado.PlanoId
                });
            }
            catch (ValidationException ex)
            {
                foreach (var error in ex.Errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamento");
                ModelState.AddModelError("", $"Erro ao processar pagamento: {ex.Message}");
                return View(model);
            }
        }

        // Métodos de validação adicionais
        private (bool IsValid, List<string> Errors) ValidarDadosSessaoPagamento()
        {
            var errors = new List<string>();

            // Verificar dados essenciais da empresa
            if (string.IsNullOrEmpty(HttpContext.Session.GetEmpresaNome()))
                errors.Add("Nome da empresa não encontrado na sessão.");

            if (string.IsNullOrEmpty(HttpContext.Session.GetEmpresaEmail()))
                errors.Add("Email da empresa não encontrado na sessão.");

            // Verificar plano selecionado
            if (HttpContext.Session.GetPlanoId() == Guid.Empty)
                errors.Add("Plano não selecionado ou inválido.");

            // Verificar pastas principais (mínimo 1 obrigatório)
            var pastasPrincipais = HttpContext.Session.GetPastasPrincipais();
            if (pastasPrincipais == null || !pastasPrincipais.Any())
                errors.Add("Pelo menos uma pasta principal deve ser configurada.");
            else if (pastasPrincipais.Any(p => string.IsNullOrWhiteSpace(p.Nome)))
                errors.Add("Uma ou mais pastas principais possuem nome inválido.");

            // Verificar usuários (mínimo 1 obrigatório)
            var usuarios = HttpContext.Session.GetUsuarios();
            if (usuarios == null || !usuarios.Any())
                errors.Add("Pelo menos um usuário deve ser cadastrado.");

            return (!errors.Any(), errors);
        }

        private bool ValidarPagamentoDTO(PagamentoDTO dto)
        {
            if (dto == null) return false;
            if (dto.PlanoId == Guid.Empty) return false;
            if (string.IsNullOrWhiteSpace(dto.EmpresaNome)) return false;
            if (string.IsNullOrWhiteSpace(dto.EmailEmpresa)) return false;
            if (dto.PastasPrincipais == null || !dto.PastasPrincipais.Any()) return false;
            if (dto.Usuarios == null || !dto.Usuarios.Any()) return false;

            return true;
        }

        [HttpGet]
        public IActionResult PagamentoConcluido(Guid empresaId, Guid pagamentoId, Guid planoId)
        {
            _logger.LogInformation("Página de pagamento concluído acessada. EmpresaId: {EmpresaId}, PagamentoId: {PagamentoId}, PlanoId: {PlanoId}, Status: {Status}",
                empresaId,
                pagamentoId,
                planoId,
                StatusPlano.Ativo);

            return View();
        }
    }
}