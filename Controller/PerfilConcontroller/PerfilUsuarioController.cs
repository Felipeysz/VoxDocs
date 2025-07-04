using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxDocs.Data.Repositories;
using VoxDocs.DTO;
using VoxDocs.Services;

namespace VoxDocs.ControllersMvc
{
    [Authorize]
    public class PerfilUsuarioController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PerfilUsuarioController> _logger;

        public PerfilUsuarioController(
            IUserRepository userRepository,
            ILogger<PerfilUsuarioController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> MeuPerfil()
        {
            try
            {
                // Obtém o nome de usuário do contexto de autenticação
                var username = User.Identity.Name;

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("Tentativa de acessar perfil sem usuário autenticado");
                    return RedirectToAction("Login", "AuthMvc");
                }

                // Busca os dados do usuário no repositório
                var user = await _userRepository.GetByUsernameAsync(username);

                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado: {Username}", username);
                    TempData["ProfileError"] = "Usuário não encontrado";
                    return RedirectToAction("Index", "Home");
                }

                // Mapeia para o DTO que a view espera
                var userInfo = new UserInfoDTO
                {
                    Usuario = user.Usuario,
                    Email = user.Email,
                    NivelPermissao = user.PermissionAccount
                };

                return View(userInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar perfil do usuário");
                TempData["ProfileError"] = "Ocorreu um erro ao carregar seu perfil";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}