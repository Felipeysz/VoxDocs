using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VoxDocs.DTO;
using VoxDocs.Services;
using VoxDocs.BusinessRules;
using Microsoft.AspNetCore.Authentication;

namespace VoxDocs.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserService _userService;
        private readonly UserBusinessRules _rules;
        private readonly IConfiguration _configuration;
        private readonly IPlanosVoxDocsService _planosService;
        private readonly IEmpresasContratanteService _empresasService;

        public UserController(
            ILogger<UserController> logger,
            IUserService userService,
            IConfiguration configuration,
            UserBusinessRules rules,
            IPlanosVoxDocsService planosService,
            IEmpresasContratanteService empresasService)
        {
            _logger = logger;
            _userService = userService;
            _configuration = configuration;
            _rules = rules;
            _planosService = planosService;
            _empresasService = empresasService;
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] DTOUser dto)
        {
            _logger.LogInformation("Register attempt for {@User}", dto);
            try
            {
                await _rules.ValidateRegisterAsync(dto);

                // Limites de plano
                int? limiteAdminPlano = null;
                int? limiteUsuarioPlano = null;
                int totalAdmins = 0;
                int totalUsers = 0;
                if (!string.IsNullOrWhiteSpace(dto.PlanoPago))
                {
                    var planos = await _planosService.GetAllPlansAsync();
                    var plano = planos.FirstOrDefault(p =>
                        p.Name.Equals(dto.PlanoPago, StringComparison.OrdinalIgnoreCase));
                    if (plano != null)
                    {
                        limiteAdminPlano = plano.LimiteAdmin;
                        limiteUsuarioPlano = plano.LimiteUsuario;

                        var usersDoPlano = await _userService.GetUsersAsync();
                        totalAdmins = usersDoPlano.Count(u =>
                            u.PlanoPago == dto.PlanoPago && u.PermissionAccount == "admin");
                        totalUsers = usersDoPlano.Count(u =>
                            u.PlanoPago == dto.PlanoPago && u.PermissionAccount == "user");

                        if (dto.PermissionAccount == "admin"
                            && limiteAdminPlano.HasValue
                            && totalAdmins >= limiteAdminPlano.Value)
                        {
                            return BadRequest(new
                            {
                                mensagem = $"Limite de administradores atingido para este plano ({limiteAdminPlano.Value})."
                            });
                        }

                        if (dto.PermissionAccount == "user"
                            && limiteUsuarioPlano.HasValue
                            && totalUsers >= limiteUsuarioPlano.Value)
                        {
                            return BadRequest(new
                            {
                                mensagem = $"Limite de usuários atingido para este plano ({limiteUsuarioPlano.Value})."
                            });
                        }
                    }
                }

                // Preenche LimiteUsuario e LimiteAdmin conforme PermissionAccount
                string? limiteUsuario = null;
                string? limiteAdmin = null;
                if (dto.PermissionAccount == "admin" && limiteAdminPlano.HasValue)
                {
                    limiteAdmin = $"{totalAdmins + 1}/{limiteAdminPlano.Value}";
                }
                else if (dto.PermissionAccount == "user" && limiteUsuarioPlano.HasValue)
                {
                    limiteUsuario = $"{totalUsers + 1}/{limiteUsuarioPlano.Value}";
                }
                dto.LimiteUsuario = limiteUsuario;
                dto.LimiteAdmin = limiteAdmin;

                var user = await _userService.RegisterUserAsync(dto);

                return Ok(new
                {
                    user.Id,
                    user.Usuario,
                    user.Email,
                    user.PermissionAccount,
                    LimiteUsuario = limiteUsuario,
                    LimiteAdmin = limiteAdmin,
                    mensagem = "Usuário criado com sucesso!"
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { mensagem = "Erro interno.", detalhes = ex.Message });
            }
        }

        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] DTOUserLogin dto)
        {
            _logger.LogInformation("Tentativa de login para {Usuário}", dto.Usuario);
            try
            {
                _rules.ValidateLoginDto(dto);
                var principal = await _userService.ValidateUserAsync(dto);

                var user = await _userService.GetUserByUsernameAsync(dto.Usuario);
                if (user == null)
                    throw new KeyNotFoundException("Usuário não encontrado.");

                var authProps = new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc   = DateTimeOffset.UtcNow.AddHours(2),
                    AllowRefresh = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProps
                );

                return Ok(new
                {
                    usuario = user.Usuario,
                    email = user.Email,
                    permissionAccount = user.PermissionAccount,
                    empresaContratante = user.EmpresaContratante,
                    mensagem = "Login realizado com sucesso!"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(new { mensagem = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return Unauthorized(new { mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { mensagem = "Erro interno.", detalhes = ex.Message });
            }
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { mensagem = "Logout realizado com sucesso." });
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var list = await _userService.GetUsersAsync();
            return Ok(list.Select(u => new { u.Id, u.Usuario, u.PermissionAccount }));
        }

        [HttpPut("{id}"), Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] DTOUser dto)
        {
            _logger.LogInformation("Update attempt for {UserId}", id);
            try
            {
                var loggedUser = User.Identity?.Name;
                var isAdmin = User.IsInRole("admin");

                if (!isAdmin && dto.Usuario != loggedUser)
                    return Forbid("Você não tem permissão para editar este usuário.");

                dto.Usuario = dto.Usuario.Trim();
                _rules.ValidateUpdate(dto);

                var updatedUser = await _userService.UpdateUserAsync(dto);
                return Ok(new
                {
                    updatedUser.Id,
                    updatedUser.Usuario,
                    updatedUser.PermissionAccount,
                    mensagem = "Usuário atualizado com sucesso!"
                });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(new { mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { mensagem = "Erro interno.", detalhes = ex.Message });
            }
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var loggedUser = User.Identity?.Name;
                var isAdmin = User.IsInRole("admin");

                if (!isAdmin)
                {
                    var user = await _userService.GetUserByIdAsync(id);
                    if (user == null || user.Usuario != loggedUser)
                        return Forbid("Você não tem permissão para excluir este usuário.");
                }

                var userToDelete = await _userService.GetUserByIdAsync(id);
                _rules.ValidateDelete(id);
                await _userService.DeleteUserAsync(id);

                return Ok(new { mensagem = $"Usuário com ID {id} deletado com sucesso!" });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return NotFound(new { mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return BadRequest(new { mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, new { mensagem = "Erro interno.", detalhes = ex.Message });
            }
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest(new { mensagem = "Usuário não informado." });

            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return NotFound(new { mensagem = "Usuário não encontrado." });

            var dto = new UserInfoDTO
            {
                Usuario = user.Usuario,
                Email = user.Email,
                PermissionAccount = user.PermissionAccount
            };
            return Ok(dto);
        }

        [HttpGet]
        public async Task<IActionResult> GetQuantidadeUsers()
        {
            var users = await _userService.GetUsersAsync();
            var agrupado = users
                .GroupBy(u => u.EmpresaContratante ?? "Não Informado")
                .Select(g => new UsuarioTotalEmpresa
                {
                    NomeEmpresa = g.Key,
                    TotalUsuarios = g.Count()
                })
                .ToList();

            return Ok(agrupado);
        }
    }
}
