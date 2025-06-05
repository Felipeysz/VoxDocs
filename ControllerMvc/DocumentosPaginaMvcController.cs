using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net;
using VoxDocs.Models.ViewModels;
using VoxDocs.DTO;
using VoxDocs.Services;
using Azure.Storage.Blobs;

namespace VoxDocs.Controllers
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DocumentosPaginaMvcController : Controller
    {
        private readonly IDocumentoService _documentoService;
        private readonly IPastaPrincipalService _pastaPrincipalService;
        private readonly ISubPastaService _subPastaService;
        private readonly HttpClient _httpClient;
        private readonly IUserService _userService;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public DocumentosPaginaMvcController(
            IDocumentoService documentoService,
            IPastaPrincipalService pastaPrincipalService,
            ISubPastaService subPastaService,
            IHttpClientFactory httpClientFactory,
            IUserService userService,
            IConfiguration configuration,
            BlobServiceClient blobServiceClient)
        {
            _documentoService = documentoService;
            _pastaPrincipalService = pastaPrincipalService;
            _subPastaService = subPastaService;
            _httpClient = httpClientFactory.CreateClient("VoxDocsApi");
            _userService = userService;
            _blobServiceClient = blobServiceClient;
            _containerName = configuration["AzureBlobStorage:ContainerName"];
        }

        [HttpGet]
        public async Task<IActionResult> DocumentosPagina(string pastaPrincipalNome = null, string subPastaNome = null)
        {
            var empresaUsuario = User.FindFirst("Empresa")?.Value;
            if (string.IsNullOrEmpty(empresaUsuario))
            {
                TempData["ErrorMessage"] = "Não foi possível identificar a empresa do usuário.";
                return View(new DocumentosViewModel());
            }

            var pastasPrincipais = (await _pastaPrincipalService.GetByEmpresaAsync(empresaUsuario))
                .Select(p => new DTOPastaPrincipal { NomePastaPrincipal = p.NomePastaPrincipal })
                .DistinctBy(p => p.NomePastaPrincipal)
                .ToList();

            var subPastas = Enumerable.Empty<DTOSubPasta>();
            var documentos = Enumerable.Empty<DTODocumentoCreate>();

            if (!string.IsNullOrEmpty(pastaPrincipalNome))
            {
                subPastas = (await _subPastaService.GetSubChildrenAsync(pastaPrincipalNome))
                    .Select(s => new DTOSubPasta { NomeSubPasta = s.NomeSubPasta })
                    .DistinctBy(s => s.NomeSubPasta)
                    .ToList();

                if (!string.IsNullOrEmpty(subPastaNome))
                {
                    documentos = (await _documentoService.GetAllAsync())
                        .Where(d => d.EmpresaContratante == empresaUsuario &&
                                    d.NomePastaPrincipal == pastaPrincipalNome &&
                                    d.NomeSubPasta == subPastaNome)
                        .ToList();
                }
            }

            var viewModel = new DocumentosViewModel
            {
                PastaPrincipais = pastasPrincipais,
                SelectedPastaPrincipalNome = pastaPrincipalNome,
                SelectedSubPastaNome = subPastaNome,
                SubPastas = subPastas,
                Documentos = documentos
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string token)
        {
            try
            {
                Console.WriteLine($"[Delete] Iniciando exclusão para ID: {id}");
                Console.WriteLine($"[Delete] Token recebido: {token ?? "null"}");

                // Valida token e obtém documento
                var documento = await _documentoService.GetByIdAsync(id, token);
                Console.WriteLine($"[Delete] Documento encontrado: {documento.NomeArquivo}");

                // Exclusão direta via serviço (já trata blob e DB)
                await _documentoService.DeleteAsync(id, token);
                Console.WriteLine($"[Delete] Documento excluído com sucesso");

                return Json(new { success = true, message = "Documento excluído com sucesso." });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[Delete] Acesso não autorizado: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[Delete] Documento não encontrado: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Delete] ERRO INESPERADO: {ex}");
                return Json(new
                {
                    success = false,
                    message = "Erro inesperado: " + ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateToken(string nomeArquivo, string token)
        {
            try
            {
                // Validação direta via serviço
                bool isValid = await _documentoService.ValidateTokenDocumentoAsync(nomeArquivo, token);
                return Ok(new { sucesso = isValid });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { sucesso = false, mensagem = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPorNome(string nomeArquivo, string token = null)
        {
            try
            {
                // Download via serviço (já inclui validação)
                var result = await _documentoService.DownloadDocumentoProtegidoAsync(nomeArquivo, token);
                return File(result.stream, result.contentType, nomeArquivo);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Edit([FromForm] DocumentoUpdateDto dto, [FromForm] string? tokenSeguranca)
        {
            // Preenche o campo obrigatório com o usuário logado ANTES da validação
            Console.WriteLine($"[Edit] Iniciando edição do documento ID: {dto?.Id}");
            Console.WriteLine($"[Edit] Token recebido: {tokenSeguranca ?? "null"}");
            Console.WriteLine($"[Edit] Novo arquivo: {(dto.NovoArquivo != null ? "Presente" : "Ausente")}");
            Console.WriteLine($"[Edit] Descrição: {dto.Descrição}");
            Console.WriteLine($"[Edit] UsuárioUltimaAlteracao: {dto.UsuarioUltimaAlteracao}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                Console.WriteLine($"[Edit] Erros de validação: {string.Join(", ", errors)}");
                return Json(new { success = false, message = "Erro de validação.", errors });
            }

            try
            {
                var userName = User.FindFirstValue(ClaimTypes.Name);
                Console.WriteLine($"[Edit] Usuário autenticado: {userName}");
                
                // Validar token e permissões
                Console.WriteLine($"[Edit] Obtendo documento original ID: {dto.Id}");
                var documentoOriginal = await _documentoService.GetByIdAsync(dto.Id, tokenSeguranca);
                Console.WriteLine($"[Edit] Documento obtido: {documentoOriginal.NomeArquivo}");
                
                // Verificar permissão para documentos confidenciais
                if (documentoOriginal.NivelSeguranca == "Confidencial" && !User.HasClaim("PermissionAccount", "admin"))
                {
                    Console.WriteLine($"[Edit] Acesso negado: usuário não é admin para documento confidencial");
                    return Json(new { success = false, message = "Apenas administradores podem editar documentos confidenciais." });
                }

                // Atualizar DTO com dados do usuário
                dto.UsuarioUltimaAlteracao = userName;
                Console.WriteLine($"[Edit] Atualizando DTO com usuário: {dto.UsuarioUltimaAlteracao}");

                // Atualizar documento via serviço
                Console.WriteLine($"[Edit] Chamando serviço de atualização");
                await _documentoService.UpdateAsync(dto);
                
                Console.WriteLine($"[Edit] Documento atualizado com sucesso");
                return Json(new { success = true, message = "Documento atualizado com sucesso." });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[Edit] ERRO Não Autorizado: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"[Edit] ERRO Argumento Inválido: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Edit] ERRO INESPERADO: {ex}");
                return Json(new { success = false, message = "Erro ao atualizar documento: " + ex.Message });
            }
        }
        private DTODocumentoCreate MapToUpdateDto(DTODocumentoCreate original, DocumentoDto dto)
        {
            return new DTODocumentoCreate
            {
                Id = original.Id,
                NomeArquivo = dto.NomeArquivo,
                NivelSeguranca = dto.NivelSeguranca,
                TokenSeguranca = dto.TokenSeguranca,
                Descrição = dto.Descrição,
                DataCriacao = original.DataCriacao,
                UsuarioCriador = original.UsuarioCriador,
                UsuarioUltimaAlteracao = User.FindFirst(ClaimTypes.Name)?.Value,
                DataUltimaAlteracao = DateTime.UtcNow,
                EmpresaContratante = original.EmpresaContratante,
                NomePastaPrincipal = original.NomePastaPrincipal,
                NomeSubPasta = original.NomeSubPasta,
                TamanhoArquivo = original.TamanhoArquivo,
                UrlArquivo = original.UrlArquivo
            };
        }
    }
}
