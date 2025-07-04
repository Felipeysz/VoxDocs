using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VoxDocs.DTO;
using VoxDocs.Services;
using VoxDocs.ViewModels;
using Microsoft.Extensions.Logging;

namespace VoxDocs.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPlanosVoxDocsService _planosService;
        private readonly ILogger<HomeController> _logger;
        private readonly IAudioProcessingService _audioService;

        public HomeController(
            IPlanosVoxDocsService planosService,
            ILogger<HomeController> logger,
            IAudioProcessingService audioService)
        {
            _planosService = planosService;
            _logger = logger;
            _audioService = audioService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string categoria = null)
        {
            try
            {
                _logger.LogInformation("Carregando página de planos");

                // Verifica se o usuário está autenticado
                var isAuthenticated = _audioService.IsUserAuthenticated();
                var username = isAuthenticated ? _audioService.GetUsername() : null;

                var viewModel = new PlanosViewModel
                {
                    CategoriaFiltro = categoria,
                    IsUserAuthenticated = isAuthenticated,
                    Username = username
                };

                if (!string.IsNullOrEmpty(categoria))
                {
                    viewModel.Planos = await _planosService.GetPlansByCategoryAsync(categoria);
                }
                else
                {
                    viewModel.Planos = await _planosService.GetAllPlansAsync();
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar planos");
                var viewModel = new PlanosViewModel
                {
                    MensagemErro = "Ocorreu um erro ao carregar os planos. Por favor, tente novamente.",
                    IsUserAuthenticated = false
                };
                return View(viewModel);
            }
        }
    }
}