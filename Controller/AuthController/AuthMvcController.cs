using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxDocs.DTO;
using VoxDocs.Services;
using VoxDocs.ViewModels;

namespace VoxDocs.ControllersMvc
{
    [AllowAnonymous]
    public class AuthMvcController : Controller
    {
        private readonly ILogger<AuthMvcController> _logger;
        private readonly IUserAuthService _userAuthService;

        public AuthMvcController(
            ILogger<AuthMvcController> logger,
            IUserAuthService userAuthService)
        {
            _logger = logger;
            _userAuthService = userAuthService;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var loginDto = new DTOLoginUsuario
                {
                    Usuario = model.Username,
                    Senha = model.Password
                };

                var principal = await _userAuthService.AuthenticateUserAsync(loginDto);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(30) : null
                    });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                _logger.LogWarning(ex, "Falha no login para o usuário {Username}", model.Username);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao tentar fazer login");
                _logger.LogError(ex, "Erro durante o login para o usuário {Username}", model.Username);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View("Home","Index");
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userAuthService.GeneratePasswordResetTokenAsync(model.Email);
                return RedirectToAction("ForgotPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao solicitar recuperação de senha para {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Se o e-mail estiver cadastrado, você receberá instruções para redefinir sua senha");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Token inválido");
            }

            var model = new ResetPasswordViewModel { Token = token };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _userAuthService.ResetPasswordWithTokenAsync(model.Token, model.NewPassword);
                return RedirectToAction("ResetPasswordConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao redefinir senha com token");
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao redefinir sua senha. O token pode ter expirado.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var username = User.Identity.Name;
                await _userAuthService.ChangePasswordAsync(username, model.OldPassword, model.NewPassword);
                
                return RedirectToAction("ChangePasswordConfirmation");
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha para o usuário {Username}", User.Identity.Name);
                ModelState.AddModelError(string.Empty, "Ocorreu um erro ao alterar sua senha");
                return View(model);
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}