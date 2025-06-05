using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxDocs.Models.ViewModels;
using VoxDocs.Services;

namespace VoxDocs.ControllerMvc
{
    [Authorize]
    public class AdminMvcController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IPastaPrincipalService _documentoService;
        private readonly IEmpresasContratanteService _empresaService; // Substitui IConsultaService

        public AdminMvcController(
            IUserService userService,
            IPlanosVoxDocsService planosService,
            IPastaPrincipalService documentoService,
            IEmpresasContratanteService empresaService) // Adicionado
        {
            _userService = userService;
            _planosService = planosService;
            _documentoService = documentoService;
            _empresaService = empresaService; // Inicializado
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var vm = new DashboardViewModel();

            // 0) Buscar usuário logado e empresa
            var userModel = await _userService.GetUserByUsernameAsync(User.Identity.Name);
            var empresa = userModel.EmpresaContratante;

            // 1) Plano contratado
            var plano = await _planosService.GetPlanByNameAsync(userModel.PlanoPago);
            vm.Plano = plano?.Name ?? "Não definido";
            vm.ArmazenamentoTotal = $"{plano?.ArmazenamentoDisponivel ?? 0} GB";

            // 2) Armazenamento usado (substituído por lógica alternativa)
            var pastas = await _documentoService.GetAllAsync();
            var storageUsed = pastas.Count(); // Usando Count() do LINQ
            vm.ArmazenamentoUsado = $"{storageUsed} GB";

            // 3) Usuários da empresa
            var todosUsuarios = await _userService.GetUsersAsync();
            var usuariosEmpresa = todosUsuarios.Where(u => u.EmpresaContratante == empresa);
            vm.UsuariosAtuais = usuariosEmpresa.Count(u => u.PermissionAccount == "user");
            vm.UsuariosPermitidos = plano?.LimiteUsuario ?? 0;
            vm.TokensDisponiveis = plano?.TokensDisponiveis ?? "Infinito";
            vm.ConsultasRealizadas = 0;
            vm.UltimaAtualizacao = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm");

            return View(vm);
        }
    }
}