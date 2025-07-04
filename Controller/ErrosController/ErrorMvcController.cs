using Microsoft.AspNetCore.Mvc;

namespace VoxDocs.Controllers
{
    public class ErrorMvcController : Controller
    {
        // Quando o usuário não estiver autenticado mas tentar acessar algo autorizado
        public IActionResult LoginNotFound()
        {
            return View();
        }

        // Para qualquer rota não mapeada / inexistente
        public IActionResult NotFoundPage()
        {
            return View();
        }

        // Para qualquer Token expirado ou inválido
        public IActionResult ErrorTokenInvalido()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ErrorOfflinePage()
        {
            return View("ErrorOfflinePage");
        }
    }
}
