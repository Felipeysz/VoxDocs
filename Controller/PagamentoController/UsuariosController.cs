using Microsoft.AspNetCore.Mvc;
using VoxDocs.DTO;
using FluentValidation.Results;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    public partial class PagamentoController : Controller
    {

        [HttpGet]
        public async Task<IActionResult> CadastroUsuarios()
        {
            if (!HttpContext.Session.HasEmpresa())
            {
                return RedirectToAction("CadastroEmpresa");
            }

            var planoId = HttpContext.Session.GetPlanoId();
            var plano = await _planosService.GetPlanByIdAsync(planoId);

            ViewBag.LimiteAdmin = plano?.LimiteAdmin ?? 0;
            ViewBag.LimiteUsuario = plano?.LimiteUsuario ?? 0;
            ViewBag.Usuarios = HttpContext.Session.GetUsuarios() ?? new List<PagamentoUsuarioDTO>();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarUsuario(PagamentoUsuarioDTO novoUsuario)
        {
            try
            {
                // Validate the user input
                var validationResult = await _pagamentoService.ValidateUserAsync(novoUsuario);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    TempData["ErrorMessage"] = "Por favor, corrija os erros no formulário";
                    return RedirectToAction("CadastroUsuarios");
                }

                // Verifica se a conta já existe no banco de dados
                bool contaExiste = await _userAuthService.AccountExistsAsync(novoUsuario.Email, novoUsuario.Usuario);
                if (contaExiste)
                {
                    TempData["ErrorMessage"] = "Já existe uma conta com este e-mail ou nome de usuário";
                    return RedirectToAction("CadastroUsuarios");
                }

                var usuarios = HttpContext.Session.GetUsuarios() ?? new List<PagamentoUsuarioDTO>();

                // Check if email already exists in session (cadastro em andamento)
                if (usuarios.Any(u => u.Email == novoUsuario.Email))
                {
                    TempData["ErrorMessage"] = "Este e-mail já está sendo cadastrado nesta sessão";
                    return RedirectToAction("CadastroUsuarios");
                }

                // Get plan limits
                var planoId = HttpContext.Session.GetPlanoId();
                var plano = await _planosService.GetPlanByIdAsync(planoId);

                var limiteAdmin = plano?.LimiteAdmin ?? 0;
                var limiteUsuario = plano?.LimiteUsuario ?? 0;

                // Check limits
                var adminCount = usuarios.Count(u => u.PermissaoConta == TipoPermissao.Admin);
                var userCount = usuarios.Count(u => u.PermissaoConta == TipoPermissao.User);

                if (novoUsuario.PermissaoConta == TipoPermissao.Admin && adminCount >= limiteAdmin)
                {
                    TempData["ErrorMessage"] = "Limite de administradores atingido";
                    return RedirectToAction("CadastroUsuarios");
                }

                if (novoUsuario.PermissaoConta == TipoPermissao.User && userCount >= limiteUsuario)
                {
                    TempData["ErrorMessage"] = "Limite de usuários comuns atingido";
                    return RedirectToAction("CadastroUsuarios");
                }

                // Add the new user
                usuarios.Add(novoUsuario);
                HttpContext.Session.SetUsuarios(usuarios);

                TempData["SuccessMessage"] = "Usuário adicionado com sucesso!";
                return RedirectToAction("CadastroUsuarios");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar usuário");
                TempData["ErrorMessage"] = "Ocorreu um erro ao processar sua solicitação";
                return RedirectToAction("CadastroUsuarios");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoverUsuario(string email)
        {
            var usuarios = HttpContext.Session.GetUsuarios() ?? new List<PagamentoUsuarioDTO>();
            var usuario = usuarios.FirstOrDefault(u => u.Email == email);

            if (usuario != null)
            {
                usuarios.Remove(usuario);
                HttpContext.Session.SetUsuarios(usuarios);
                TempData["SuccessMessage"] = "Usuário removido com sucesso!";
            }
            else
            {
                TempData["ErrorMessage"] = "Usuário não encontrado";
            }

            return RedirectToAction("CadastroUsuarios");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FinalizarCadastroUsuarios()
        {
            var usuarios = HttpContext.Session.GetUsuarios() ?? new List<PagamentoUsuarioDTO>();

            if (!usuarios.Any(u => u.PermissaoConta == TipoPermissao.Admin))
            {
                TempData["ErrorMessage"] = "É necessário ter pelo menos um administrador";
                return RedirectToAction("CadastroUsuarios");
            }

            return RedirectToAction("FinalizarPagamento");
        }
    }
}