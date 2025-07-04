using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using VoxDocs.DTO;
using VoxDocs.Models.ViewModels;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IDocumentosService _documentosService;
        private readonly IPastaPrincipalService _pastaPrincipalService;
        private readonly ISubPastaService _subPastaService;
        private readonly IUserAuthService _userService;
        private readonly IEmpresasContratanteService _empresasContratanteService;
        private readonly ILogger<UploadController> _logger;

        public UploadController(
            IDocumentosService documentosService,
            IPastaPrincipalService pastaPrincipalService,
            ISubPastaService subPastaService,
            IUserAuthService userService,
            IEmpresasContratanteService empresasContratanteService,
            ILogger<UploadController> logger)
        {
            _documentosService = documentosService;
            _pastaPrincipalService = pastaPrincipalService;
            _subPastaService = subPastaService;
            _userService = userService;
            _empresasContratanteService = empresasContratanteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                _logger.LogInformation("User accessing upload: {Username}", userName);

                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user == null)
                {
                    _logger.LogWarning("User not found in database: {Username}", userName);
                    return RedirectToAction("Error", "Home");
                }

                // Verificar permissões
                var isAdmin = user.PermissionAccount == "Admin";
                var empresaNome = user.EmpresaContratante;

                // Obter empresa por nome usando o serviço injetado
                var empresa = await _empresasContratanteService.ObterPorNomeAsync(empresaNome);
                if (empresa == null)
                {
                    _logger.LogError("Empresa não encontrada: {EmpresaNome}", empresaNome);
                    return RedirectToAction("Error", "Home");
                }

                // Obter TODAS as pastas principais da empresa
                var pastasPrincipais = await _pastaPrincipalService.ObterTodasPorEmpresaIdAsync(empresa.Id);

                if (!pastasPrincipais.Any())
                {
                    _logger.LogWarning("Nenhuma pasta principal encontrada para a empresa: {EmpresaId}", empresa.Id);
                    return RedirectToAction("Error", "Home");
                }

                var viewModel = new UploadDocumentoViewModel
                {
                    PastaPrincipais = pastasPrincipais.ToList(), // Todas as pastas
                    IsAdmin = isAdmin
                };

                ViewBag.Usuario = user.Usuario;
                ViewBag.Empresa = empresaNome;
                ViewBag.IsAdmin = isAdmin;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading upload page");
                return RedirectToAction("Error", "Home");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([FromForm] UploadDocumentoViewModel model)
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                _logger.LogInformation("Iniciando upload de documento pelo usuário: {Username}", userName);

                // Obter usuário completo
                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado durante upload: {Username}", userName);
                    return Json(new { success = false, message = "Usuário não encontrado." });
                }

                // Verificar permissões
                var isAdmin = user.PermissionAccount == "Admin";

                // Obter empresa do usuário
                var empresa = await _empresasContratanteService.ObterPorNomeAsync(user.EmpresaContratante);
                if (empresa == null)
                {
                    _logger.LogError("Empresa não encontrada: {EmpresaNome}", user.EmpresaContratante);
                    return Json(new { success = false, message = "Empresa não encontrada." });
                }

                // Validar se foi selecionada uma pasta principal
                if (!model.SelectedPastaPrincipalId.HasValue)
                {
                    _logger.LogWarning("Nenhuma pasta principal selecionada pelo usuário: {Username}", userName);
                    return Json(new
                    {
                        success = false,
                        message = "Selecione uma pasta principal para o documento."
                    });
                }

                // Obter pasta principal
                var pastaPrincipal = await _pastaPrincipalService.ObterPorIdAsync(model.SelectedPastaPrincipalId.Value);
                if (pastaPrincipal == null || pastaPrincipal.EmpresaId != empresa.Id)
                {
                    _logger.LogWarning("Tentativa de upload para pasta não autorizada. Usuário: {Username}, Pasta: {PastaId}",
                        userName, model.SelectedPastaPrincipalId);
                    return Json(new { success = false, message = "Pasta não autorizada." });
                }

                // Validação manual dos campos obrigatórios
                var errors = new List<string>();

                if (model.Arquivo == null || model.Arquivo.Length == 0)
                    errors.Add("Por favor, selecione um arquivo");

                if (string.IsNullOrWhiteSpace(model.Descricao))
                    errors.Add("Por favor, insira uma descrição");

                if (string.IsNullOrWhiteSpace(model.NivelSeguranca))
                    errors.Add("Por favor, selecione o nível de segurança");

                if (model.NivelSeguranca != "Publico" && string.IsNullOrWhiteSpace(model.TokenSeguranca))
                    errors.Add("Token de segurança é obrigatório para este nível");

                if (errors.Any())
                {
                    _logger.LogWarning("Erros de validação no upload: {Errors}", string.Join(", ", errors));
                    return Json(new
                    {
                        success = false,
                        message = "Erro de validação nos dados do documento.",
                        errors
                    });
                }

                // Verificar subpasta se existir
                if (model.SelectedSubPastaId.HasValue)
                {
                    var subPasta = await _subPastaService.ObterSubPastaPorIdAsync(model.SelectedSubPastaId.Value);
                    if (subPasta == null || subPasta.PastaPrincipalId != model.SelectedPastaPrincipalId)
                    {
                        _logger.LogWarning("Subpasta inválida. ID: {SubPastaId}, Pasta Principal: {PastaPrincipalId}",
                            model.SelectedSubPastaId, model.SelectedPastaPrincipalId);
                        return Json(new { success = false, message = "Subpasta inválida." });
                    }
                }

                // Criar DTO para upload
                var dto = new DocumentoCreateDTO
                {
                    Id = Guid.NewGuid(),
                    NomeArquivo = Path.GetFileNameWithoutExtension(model.Arquivo.FileName),
                    DescricaoArquivo = model.Descricao,
                    IdPastaPrincipal = model.SelectedPastaPrincipalId.Value,
                    IdSubPasta = model.SelectedSubPastaId,
                    NivelPrioridade = model.NivelSeguranca switch
                    {
                        "Publico" => 1,
                        "Restrito" => 2,
                        "Confidencial" => 3,
                        "Prioritario" => 4,
                        "Urgente" => 5,
                        _ => 1
                    },
                    TokenAcesso = model.NivelSeguranca != "Publico" ? model.TokenSeguranca : null,
                    UsuarioCriacao = user.Usuario,
                    ContentType = model.Arquivo.ContentType,
                    DataCriacao = DateTime.UtcNow
                };

                // Realizar upload
                using var stream = model.Arquivo.OpenReadStream();
                var resultadoUpload = await _documentosService.UploadDocumentoAsync(dto, stream);

                _logger.LogInformation("Upload concluído com sucesso. Usuário: {Username}, Documento: {DocumentoId}",
                    userName, resultadoUpload.Id);

                return Json(new
                {
                    success = true,
                    message = "Documento enviado com sucesso!",
                    documentoId = resultadoUpload.Id,
                    nivelPrioridade = dto.NivelPrioridade,
                    tipoDocumento = model.NivelSeguranca
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Acesso não autorizado durante upload");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o upload de documento");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocorreu um erro durante o upload do documento.",
                    detalhes = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubPastas(Guid pastaPrincipalId)
        {
            try
            {
                // 1. Verificar autenticação do usuário
                var userName = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(userName))
                {
                    _logger.LogWarning("Tentativa de acesso sem autenticação");
                    return Json(new { success = false, message = "Usuário não autenticado." });
                }

                // 2. Obter usuário do banco de dados
                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user == null)
                {
                    _logger.LogWarning("Usuário não encontrado: {UserName}", userName);
                    return Json(new { success = false, message = "Usuário não encontrado." });
                }

                // 3. Validar empresa contratante
                if (string.IsNullOrEmpty(user.EmpresaContratante))
                {
                    _logger.LogWarning("Usuário sem empresa contratante: {UserId}", user.Id);
                    return Json(new { success = false, message = "Usuário não possui empresa contratante associada." });
                }

                // 4. Obter empresa por nome
                var empresa = await _empresasContratanteService.ObterPorNomeAsync(user.EmpresaContratante);
                if (empresa == null)
                {
                    _logger.LogError("Empresa não encontrada: {EmpresaNome}", user.EmpresaContratante);
                    return Json(new { success = false, message = "Empresa não encontrada." });
                }

                // 5. Verificar se a pasta principal pertence à empresa do usuário
                var pastaPrincipal = await _pastaPrincipalService.ObterPorIdAsync(pastaPrincipalId);
                if (pastaPrincipal == null || pastaPrincipal.EmpresaId != empresa.Id)
                {
                    _logger.LogWarning("Tentativa de acesso a pasta não autorizada. Usuário: {Username}, Pasta: {PastaId}",
                        userName, pastaPrincipalId);
                    return Json(new { success = false, message = "Pasta não autorizada." });
                }

                // 6. Obter subpastas para a pasta principal específica
                var subPastas = await _subPastaService.ObterPorPastaPrincipalIdAsync(pastaPrincipalId);
                if (subPastas == null || !subPastas.Any())
                {
                    _logger.LogInformation("Nenhuma subpasta encontrada para a pasta principal {PastaId}", pastaPrincipalId);
                    return Json(new { success = true, data = new List<object>() });
                }

                // 7. Retornar resultado
                return Json(new
                {
                    success = true,
                    data = subPastas.Select(s => new
                    {
                        id = s.Id,
                        nomeSubPasta = s.NomeSubPasta
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar subpastas para pasta principal {PastaPrincipalId}", pastaPrincipalId);
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Erro interno ao obter subpastas. Detalhes: {ex.Message}"
                });
            }
        }
    }
}