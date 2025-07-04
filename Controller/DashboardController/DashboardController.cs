using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using VoxDocs.Services;
using VoxDocs.Models.ViewModels;

namespace VoxDocs.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IUserAuthService _userService;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IDocumentosService _documentoService;

        public DashboardController(
            ILogger<DashboardController> logger,
            IUserAuthService userService,
            IPlanosVoxDocsService planosService,
            IDocumentosService documentoService)
        {
            _logger = logger;
            _userService = userService;
            _planosService = planosService;
            _documentoService = documentoService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userModel = await _userService.GetUserByUsernameAsync(User.Identity.Name);
                var plano = await _planosService.GetPlanByNameAsync(userModel.PlanoPago);

                var vm = new DashboardViewModel
                {
                    // Informações do Usuário
                    Usuario = userModel.Usuario,
                    Email = userModel.Email,
                    EmpresaContratante = userModel.EmpresaContratante,

                    // Informações do Plano
                    Plano = plano?.Nome ?? "Não definido",
                    ArmazenamentoTotal = (plano?.ArmazenamentoDisponivel ?? 0).ToString("N0") + " GB",

                    // Timestamp
                    UltimaAtualizacao = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar dashboard");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar o dashboard. Por favor, tente novamente.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}