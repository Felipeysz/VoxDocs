using Microsoft.AspNetCore.Mvc;
using VoxDocs.PagamentoViewModel;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    public partial class PagamentoController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelecionarPlano(Guid planoId, string planoNome, decimal planoPreco)
        {
            HttpContext.Session.SetPlano(planoId, planoNome, planoPreco);
            return RedirectToAction("CadastroEmpresa");
        }

        [HttpGet]
        public IActionResult CadastroEmpresa()
        {
            if (!HttpContext.Session.HasPlano())
            {
                return RedirectToAction("Index", "Index");
            }

            return View(new CadastroEmpresaViewModel
            {
                PlanoId = HttpContext.Session.GetPlanoId(),
                NomePlano = HttpContext.Session.GetPlanoNome(),
                EmpresaNome = HttpContext.Session.GetEmpresaNome(),
                EmailEmpresa = HttpContext.Session.GetEmpresaEmail()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastroEmpresa(CadastroEmpresaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.PlanoId = HttpContext.Session.GetPlanoId();
                model.NomePlano = HttpContext.Session.GetPlanoNome();
                return View(model);
            }

            if (await _pagamentoService.VerificarEmpresaExistenteAsync(model.EmpresaNome, model.EmailEmpresa))
            {
                ModelState.AddModelError(string.Empty, "Empresa já cadastrada com este nome/e-mail.");
                return View(model);
            }

            HttpContext.Session.SetEmpresa(model.EmpresaNome, model.EmailEmpresa);
            return RedirectToAction("CadastroPastaPrincipal");
        }

        [HttpGet]
        public async Task<IActionResult> VerificarNomeEmpresa(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return Json(new { existe = false });
            }

            bool existe = await _pagamentoService.VerificarEmpresaExistenteAsync(nome, null);
            return Json(new { existe });
        }

        [HttpGet]
        public async Task<IActionResult> VerificarEmailEmpresa(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Json(new { existe = false });
            }

            bool existe = await _pagamentoService.VerificarEmpresaExistenteAsync(null, email);
            return Json(new { existe });
        }
    }
}