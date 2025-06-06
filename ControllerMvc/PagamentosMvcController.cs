using Microsoft.AspNetCore.Mvc;
using VoxDocs.Services;
using VoxDocs.ViewModels;
using VoxDocs.DTO;
using VoxDocs.Data;
using System.Linq;

namespace VoxDocs.Controllers
{
    public class PagamentosMvcController : Controller
    {
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IPagamentoCartaoFalsoService _pagamentoCartaoService;
        private readonly IPagamentoPixFalsoService _pagamentoPixService;
        private readonly IConfiguration _configuration;
        private readonly IEmpresasContratanteService _empresaService;
        private readonly IPastaPrincipalService _pastaService;
        private readonly ISubPastaService _subPastaService;
        private readonly IUserService _userService;
        private readonly VoxDocsContext _context;
        private readonly IPagamentoConcluidoService _pagamentoConcluidoService;

        public PagamentosMvcController(
            IPlanosVoxDocsService planosService,
            IPagamentoCartaoFalsoService pagamentoCartaoService,
            IPagamentoPixFalsoService pagamentoPixService,
            IConfiguration configuration,
            IEmpresasContratanteService empresaService,
            IPastaPrincipalService pastaService,
            ISubPastaService subPastaService,
            IUserService userService,
            VoxDocsContext context,
            IPagamentoConcluidoService pagamentoConcluidoService)
        {
            _planosService = planosService;
            _pagamentoCartaoService = pagamentoCartaoService;
            _pagamentoPixService = pagamentoPixService;
            _configuration = configuration;
            _empresaService = empresaService;
            _pastaService = pastaService;
            _subPastaService = subPastaService;
            _userService = userService;
            _context = context;
            _pagamentoConcluidoService = pagamentoConcluidoService;
        }

        [HttpGet]
        public IActionResult ConfirmarPagamentoCartao(string token)
        {
            ViewData["Token"] = token;
            return View("ConfirmarPagamentoCartao");
        }

        [HttpGet]
        public IActionResult ConfirmarPagamentoPix()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var existingUser = await _userService.GetUserByEmailAsync(email);
            return Json(existingUser != null);
        }

        [HttpGet]
        public IActionResult GetCurrentPlan()
        {
            var planoNome = HttpContext.Session.GetString("PlanoSelecionado");
            return Json(new { planoNome });
        }
        [HttpGet]
        public async Task<IActionResult> Step(int step)
        {
            var planoNome = HttpContext.Session.GetString("PlanoSelecionado");
            if (string.IsNullOrEmpty(planoNome))
                return RedirectToAction("SelecionarPlano");

            ViewBag.PlanoPago = planoNome;

            switch (step)
            {
                case 1:
                    return PartialView("Step1", new DTOCadastroEmpresa { PlanoPago = planoNome });
                case 2:
                    return PartialView("Step2", new DTOCadastroEmpresa { PlanoPago = planoNome });
                case 3:
                    var plano3 = await _planosService.GetPlanByNameAsync(planoNome);
                    if (plano3 == null)
                        return RedirectToAction("SelecionarPlano");
                    ViewBag.LimiteAdmin = plano3.LimiteAdmin;
                    return PartialView("Step3", new DTOCadastroEmpresa { PlanoPago = planoNome });
                case 4:
                    var plano4 = await _planosService.GetPlanByNameAsync(planoNome);
                    if (plano4 == null)
                        return RedirectToAction("SelecionarPlano");
                    ViewBag.LimiteUsuarios = plano4.LimiteUsuario;
                    return PartialView("Step4", new DTOCadastroEmpresa { PlanoPago = planoNome });
                case 5:
                    return PartialView("Step5", new DTOCadastroEmpresa { PlanoPago = planoNome });
                default:
                    return BadRequest($"Step inválido: {step}");
            }
        }

        public IActionResult SelecionarPlano(string planoNome)
        {
            HttpContext.Session.SetString("PlanoSelecionado", planoNome);
            return RedirectToAction("CadastroEmpresa");
        }

        public async Task<IActionResult> CadastroEmpresa()
        {
            var planoNome = HttpContext.Session.GetString("PlanoSelecionado");
            if (string.IsNullOrEmpty(planoNome))
            {
                TempData["ErrorMessage"] = "Plano inválido ou não encontrado.";
                return RedirectToAction("Index", "IndexMvc");
            }

            var plano = await _planosService.GetPlanByNameAsync(planoNome);
            if (plano == null)
            {
                TempData["ErrorMessage"] = "Plano inválido ou não encontrado.";
                return RedirectToAction("Index", "IndexMvc");
            }

            ViewBag.PlanoNome = planoNome;
            ViewBag.LimiteUsuarios = plano.LimiteUsuario;
            ViewBag.LimiteAdmin = plano.LimiteAdmin;
            ViewBag.PlanoSelecionado = planoNome;

            var dto = new DTOCadastroEmpresa
            {
                Usuarios = new List<UsuarioCadastro>
                {
                    new UsuarioCadastro { PermissionAccount = "Admin" }
                },
                PlanoPago = planoNome // Adicionado para garantir que o DTO tenha o plano
            };

            return View(dto);
        }

        public async Task<IActionResult> Pagamentos(string empresa, string planoSelecionado)
        {
            if (string.IsNullOrEmpty(planoSelecionado))
                return BadRequest("Plano não informado.");

            var planos = await _planosService.GetPlansByCategoryAsync(planoSelecionado);
            if (planos == null || !planos.Any())
                return View("NenhumPlano", planoSelecionado);

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            ViewData["BaseUrl"] = baseUrl;

            var viewModel = new PagamentosViewModel
            {
                Categoria = planoSelecionado,
                Planos = planos,
                Empresa = empresa
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> FinalizarCadastro(DTOCadastroEmpresa dto)
        {
            var plano = await _planosService.GetPlanByNameAsync(dto.PlanoPago);
            if (plano == null)
                return RedirectToAction("SelecionarPlano");

            // ✅ Tratamento de valores nulos com operador ??
            int limiteUsuarios = plano.LimiteUsuario ?? 0;
            int limiteAdmin = plano.LimiteAdmin ?? 0;

            // ✅ Validação de limite de usuários
            if (dto.Usuarios.Count > limiteUsuarios)
                ModelState.AddModelError("", $"O plano permite apenas {limiteUsuarios} usuários.");

            // ✅ Validação de limite de administradores
            int adminCount = dto.Usuarios.Count(u => u.PermissionAccount == "Admin");
            int maxAdmins = Math.Min(limiteUsuarios, limiteAdmin);
            if (adminCount > maxAdmins)
                ModelState.AddModelError("", $"O plano permite no máximo {maxAdmins} administradores.");

            if (!ModelState.IsValid)
            {
                ViewBag.PlanoNome = dto.PlanoPago;
                ViewBag.LimiteUsuarios = limiteUsuarios;
                ViewBag.LimiteAdmin = limiteAdmin;
                return View("CadastroEmpresa", dto);
            }

            try
            {
                // ✅ Criação da empresa
                await _empresaService.CreateAsync(new DTOEmpresasContratante
                {
                    EmpresaContratante = dto.EmpresaContratante,
                    Email = dto.EmailEmpresa
                });

                // ✅ Criação da pasta principal
                await _pastaService.CreateAsync(new DTOPastaPrincipalCreate
                {
                    NomePastaPrincipal = dto.NomePastaPrincipal,
                    EmpresaContratante = dto.EmpresaContratante
                });

                // ✅ Criação da subpasta
                await _subPastaService.CreateAsync(new DTOSubPastaCreate
                {
                    NomeSubPasta = dto.NomeSubPasta,
                    NomePastaPrincipal = dto.NomePastaPrincipal,
                    EmpresaContratante = dto.EmpresaContratante
                });

                // ✅ Criação dos usuários
                foreach (var usuario in dto.Usuarios)
                {
                    await _userService.RegisterUserAsync(new DTOUser
                    {
                        Usuario = usuario.Usuario,
                        Email = usuario.Email,
                        Senha = usuario.Senha,
                        PermissionAccount = usuario.PermissionAccount,
                        EmpresaContratante = dto.EmpresaContratante,
                        PlanoPago = dto.PlanoPago
                    });
                }

                // ✅ Redireciona sem passar planoSelecionado na URL
                return RedirectToAction("Pagamentos", new { empresa = dto.EmpresaContratante });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao finalizar cadastro: {ex.Message}");
                ViewBag.PlanoNome = dto.PlanoPago;
                ViewBag.LimiteUsuarios = limiteUsuarios;
                ViewBag.LimiteAdmin = limiteAdmin;
                return View("CadastroEmpresa", dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessarPagamento([FromBody] PagamentoCartaoRequestDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.PlanoPago))
                return BadRequest("Dados de pagamento ou plano ausentes.");

            var plano = await _planosService.GetPlanByNameAsync(dto.PlanoPago);
            if (plano == null)
                return BadRequest("Plano não encontrado.");

            await _pagamentoCartaoService.ProcessarPagamentoCartaoFalsoAsync(dto);

            var pagamentoConcluido = new PagamentoConcluidoCreateDto
            {
                EmpresaContratante = dto.EmpresaContratante,
                MetodoPagamento = "Cartão",
                DataPagamento = DateTime.UtcNow,
                DataExpiracao = DateTime.UtcNow.AddYears(1)
            };
            var criado = await _pagamentoConcluidoService.CriarPagamentoConcluidoAsync(pagamentoConcluido);

            return Ok(new { pagamentoConcluidoId = criado.Id });
        }

        [HttpPost]
        public async Task<IActionResult> ProcessarPagamentoPix([FromBody] PagamentoPixRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Dados de pagamento Pix ausentes.");

            var pixResponse = await _pagamentoPixService.GerarPixAsync(dto);

            var pagamentoConcluido = new PagamentoConcluidoCreateDto
            {
                EmpresaContratante = dto.EmpresaContratante,
                MetodoPagamento = "Pix",
                DataPagamento = DateTime.UtcNow,
                DataExpiracao = DateTime.UtcNow.AddMinutes(10)
            };
            var criado = await _pagamentoConcluidoService.CriarPagamentoConcluidoAsync(pagamentoConcluido);

            return Ok(new
            {
                pagamentoConcluidoId = criado.Id,
                qrCode = pixResponse.qrCodeUrl,
                pagamentoPixId = pixResponse.pagamentoPixId
            });
        }
    }
}