using Microsoft.AspNetCore.Mvc;
using VoxDocs.Models.ViewModels;
using VoxDocs.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using VoxDocs.DTO;
using Microsoft.Extensions.Logging;
using FluentValidation;

namespace VoxDocs.Controllers
{
    [Authorize]
    public class DocumentosController : Controller
    {
        private readonly IDocumentosService _documentosService;
        private readonly IPastaPrincipalService _pastaPrincipalService;
        private readonly ISubPastaService _subPastaService;
        private readonly IUserAuthService _userService;
        private readonly ILogger<DocumentosController> _logger;
        private readonly IEmpresasContratanteService _empresasContratanteService;

        public DocumentosController(
            IDocumentosService documentosService,
            IPastaPrincipalService pastaPrincipalService,
            ISubPastaService subPastaService,
            IUserAuthService userService,
            IEmpresasContratanteService empresasContratanteService,
            ILogger<DocumentosController> logger)
        {
            _documentosService = documentosService;
            _pastaPrincipalService = pastaPrincipalService;
            _subPastaService = subPastaService;
            _userService = userService;
            _empresasContratanteService = empresasContratanteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> DocumentosPagina(Guid? pastaPrincipalId = null, Guid? subPastaId = null)
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userService.GetUserByUsernameAsync(userName);

                if (user == null || string.IsNullOrEmpty(user.EmpresaContratante))
                {
                    TempData["ErrorMessage"] = "Usuário não associado a uma empresa.";
                    return RedirectToAction("Index", "Home");
                }

                var empresa = await _empresasContratanteService.ObterPorNomeAsync(user.EmpresaContratante);
                var pastasPrincipais = await _pastaPrincipalService.ObterTodasPorEmpresaIdAsync(empresa.Id);

                var viewModel = new DocumentosViewModel
                {
                    PastasPrincipais = pastasPrincipais.ToList(),
                    PastaPrincipalSelecionada = pastaPrincipalId,
                    SubPastaSelecionada = subPastaId,
                    IsAdmin = user.PermissionAccount == "Admin"
                };

                if (pastaPrincipalId.HasValue)
                {
                    viewModel.Documentos = (await _documentosService.ListarDocumentosPorPastaAsync(
                        pastaPrincipalId.Value,
                        subPastaId,
                        userName,
                        viewModel.IsAdmin)).ToList();

                    if (!subPastaId.HasValue)
                    {
                        viewModel.SubPastas = (await _subPastaService.ObterPorPastaPrincipalIdAsync(pastaPrincipalId.Value)).ToList();
                    }
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar página de documentos");
                TempData["ErrorMessage"] = "Ocorreu um erro ao carregar os documentos.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterSubPastas(Guid pastaPrincipalId)
        {
            try
            {
                var subPastas = await _subPastaService.ObterPorPastaPrincipalIdAsync(pastaPrincipalId);
                return Json(new
                {
                    success = true,
                    data = subPastas.Select(s => new {
                        id = s.Id,
                        nomeSubPasta = s.NomeSubPasta
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter subpastas");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Excluir([FromBody] ExcluirDocumentoRequest request)
        {
            try
            {
                _logger.LogInformation("Iniciando exclusão do documento {DocumentId}", request?.Id);

                // Validação explícita do Guid
                if (request == null || request.Id == Guid.Empty)
                {
                    _logger.LogWarning("Tentativa de exclusão com ID vazio ou request nulo");
                    return Json(new
                    {
                        success = false,
                        message = "ID do documento inválido.",
                        invalidId = true
                    });
                }

                var userName = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(userName))
                {
                    _logger.LogWarning("Usuário não autenticado tentando excluir documento");
                    return Json(new
                    {
                        success = false,
                        message = "Usuário não autenticado."
                    });
                }

                var user = await _userService.GetUserByUsernameAsync(userName);
                if (user == null)
                {
                    _logger.LogWarning("Usuário {UserName} não encontrado", userName);
                    return Json(new
                    {
                        success = false,
                        message = "Usuário não encontrado."
                    });
                }

                var isAdmin = user.PermissionAccount == "Admin";
                if (!isAdmin)
                {
                    _logger.LogWarning("Usuário {UserName} sem permissão tentando excluir documento", userName);
                    return Json(new
                    {
                        success = false,
                        message = "Acesso não autorizado."
                    });
                }

                _logger.LogInformation("Obtendo documento {DocumentId} para exclusão", request.Id);
                var documento = await _documentosService.ObterDocumentoParaEdicaoAsync(request.Id, userName, isAdmin);
                if (documento == null)
                {
                    _logger.LogWarning("Documento {DocumentId} não encontrado para exclusão", request.Id);
                    return Json(new
                    {
                        success = false,
                        message = "Documento não encontrado.",
                        notFound = true
                    });
                }

                if (documento.NivelPrioridade > 1 && string.IsNullOrEmpty(request.TokenAcesso))
                {
                    _logger.LogInformation("Documento {DocumentId} requer token de segurança", request.Id);
                    return Json(new
                    {
                        success = false,
                        message = "Token de segurança é requerido para este documento.",
                        requiresToken = true
                    });
                }

                try
                {
                    _logger.LogInformation("Chamando serviço para remover documento {DocumentId}", request.Id);
                    await _documentosService.RemoverDocumentoAsync(request.Id, userName, request.TokenAcesso);
                    _logger.LogInformation("Documento {DocumentId} removido com sucesso", request.Id);

                    return Json(new
                    {
                        success = true,
                        message = "Documento excluído com sucesso!"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao chamar RemoverDocumentoAsync para o documento {DocumentId}", request.Id);
                    return Json(new
                    {
                        success = false,
                        message = "Falha ao excluir documento.",
                        errorDetails = ex.Message
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir documento {DocumentId}", request?.Id ?? Guid.Empty);
                return Json(new
                {
                    success = false,
                    message = ex is UnauthorizedAccessException ?
                        "Acesso não autorizado." : "Ocorreu um erro ao excluir o documento.",
                    errorDetails = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userService.GetUserByUsernameAsync(userName);
                var isAdmin = user?.PermissionAccount == "Admin";

                var downloadDto = await _documentosService.DownloadDocumentoAsync(id, userName, isAdmin);
                return File(downloadDto.Conteudo, downloadDto.TipoConteudo, downloadDto.NomeArquivo);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Documento não encontrado");
                TempData["ErrorMessage"] = "Documento não encontrado.";
                return RedirectToAction(nameof(DocumentosPagina));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Acesso não autorizado");
                TempData["ErrorMessage"] = "Acesso não autorizado.";
                return RedirectToAction(nameof(DocumentosPagina));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Tipo de arquivo não permitido");
                TempData["ErrorMessage"] = "Tipo de arquivo não permitido para download.";
                return RedirectToAction(nameof(DocumentosPagina));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar documento");
                TempData["ErrorMessage"] = "Ocorreu um erro ao processar o download.";
                return RedirectToAction(nameof(DocumentosPagina));
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterParaEdicao(Guid id)
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userService.GetUserByUsernameAsync(userName);
                var isAdmin = user?.PermissionAccount == "Admin";

                var documento = await _documentosService.ObterDocumentoParaEdicaoAsync(id, userName, isAdmin);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        id = documento.Id,
                        nomeArquivo = documento.NomeArquivo,
                        nivelPrioridade = documento.NivelPrioridade
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter documento para edição");
                return Json(new
                {
                    success = false,
                    message = ex is UnauthorizedAccessException ?
                        "Acesso não autorizado." : "Erro ao carregar documento."
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar([FromForm] DocumentoUpdateDTO dto)
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userService.GetUserByUsernameAsync(userName);
                var isAdmin = user?.PermissionAccount == "Admin";

                // Validação simplificada
                if (dto.Id == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "ID do documento inválido" });
                }

                // Validação do token atual para todos os níveis
                if (string.IsNullOrEmpty(dto.TokenAtual) || dto.TokenAtual.Length < 10 || dto.TokenAtual.Length > 50)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Token atual é obrigatório e deve ter entre 10 e 50 caracteres"
                    });
                }

                // Validação adicional para níveis 3+
                if (dto.NivelPrioridade >= 3)
                {
                    if (string.IsNullOrEmpty(dto.NovoToken) || dto.NovoToken.Length < 10 || dto.NovoToken.Length > 50)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Novo token é obrigatório e deve ter entre 10 e 50 caracteres"
                        });
                    }

                    if (string.IsNullOrEmpty(dto.ConfirmarToken) || dto.NovoToken != dto.ConfirmarToken)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Confirmação do token não coincide com o novo token"
                        });
                    }
                }

                // Processar o arquivo se foi enviado
                byte[] arquivoBytes = null;
                if (dto.Arquivo != null && dto.Arquivo.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await dto.Arquivo.CopyToAsync(memoryStream);
                    arquivoBytes = memoryStream.ToArray();
                }

                // Chamar o serviço
                await _documentosService.EditarDocumentoAsync(dto, userName, isAdmin);

                return Ok(new
                {
                    success = true,
                    message = "Documento atualizado com sucesso!"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Erro de autorização ao editar documento");
                return Unauthorized(new
                {
                    success = false,
                    message = "Token inválido ou permissão insuficiente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar documento");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Ocorreu um erro ao editar o documento"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detalhes(Guid id)
        {
            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                var user = await _userService.GetUserByUsernameAsync(userName);
                var isAdmin = user?.PermissionAccount == "Admin";

                var documento = await _documentosService.ObterDetalhesDocumentoAsync(id, userName, isAdmin);
                return View(documento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter detalhes do documento");
                TempData["ErrorMessage"] = ex is UnauthorizedAccessException ?
                    "Acesso não autorizado." : "Ocorreu um erro ao carregar o documento.";
                return RedirectToAction(nameof(DocumentosPagina));
            }
        }
    }

    // Classe auxiliar para o request de exclusão
    public class ExcluirDocumentoRequest
    {
        public Guid Id { get; set; }
        public string TokenAcesso { get; set; }
    }
}