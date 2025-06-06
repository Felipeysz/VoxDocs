// Controllers/IndexMvcController.cs
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using VoxDocs.Services;
using VoxDocs.ViewModels;
using VoxDocs.Models; // Necessário para acessar PlanosVoxDocsModel
using System.Collections.Generic;

namespace VoxDocs.Controllers
{
    public class IndexMvcController : Controller
    {
        private readonly IPlanosVoxDocsService _planosService;

        public IndexMvcController(IPlanosVoxDocsService planosService)
            => _planosService = planosService;

        // Método auxiliar para mapear Model para ViewModel
        private PlanoViewModel MapToViewModel(PlanosVoxDocsModel plano)
        {
            return new PlanoViewModel
            {
                Periodicidade = plano.Periodicidade?.ToString() ?? "Não definida",
                Price = plano.Price,
                LimiteUsuario = plano.LimiteUsuario,
                LimiteAdmin = plano.LimiteAdmin
            };
        }


        // GET: /IndexMvc/Index
        public async Task<IActionResult> Index()
        {
            // Verifica se há uma mensagem de erro armazenada em TempData
            if (TempData.ContainsKey("ErrorMessage"))
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            var basicos = await _planosService.GetPlansByCategoryAsync("Básico");
            var intermediarios = await _planosService.GetPlansByCategoryAsync("Intermediário");
            var avancados = await _planosService.GetPlansByCategoryAsync("Avançado");

            var vm = new PlanosIndexViewModel
            {
                Basicos = basicos.Select(MapToViewModel).ToList(),
                Intermediarios = intermediarios.Select(MapToViewModel).ToList(),
                Avancados = avancados.Select(MapToViewModel).ToList(),
                MenorPrecoBasico = basicos.Any() ? basicos.Min(p => p.Price) : 0,
                MenorPrecoIntermediario = intermediarios.Any() ? intermediarios.Min(p => p.Price) : 0,
                MenorPrecoAvancado = avancados.Any() ? avancados.Min(p => p.Price) : 0,
            };

            return View(vm);
}

    }
}
