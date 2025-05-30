using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.Storage.Blobs;
using VoxDocs.DTO;
using VoxDocs.Models.ViewModels;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    [Authorize]
    public class DocumentosMvcController : Controller
    {
        private readonly IDocumentoService _documentoService;
        private readonly IPastaPrincipalService _pastaService;
        private readonly ISubPastaService _subPastaService;
        private readonly IUserService _userService;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName;

        public DocumentosMvcController(
            IDocumentoService documentoService,
            IPastaPrincipalService pastaService,
            ISubPastaService subPastaService,
            IUserService userService,
            IConfiguration configuration,
            BlobServiceClient blobServiceClient)
        {
            _documentoService = documentoService;
            _pastaService = pastaService;
            _subPastaService = subPastaService;
            _userService = userService;
            _blobServiceClient = blobServiceClient;
            _containerName = configuration["AzureBlobStorage:ContainerName"];
        }

        [HttpGet]
        public async Task<IActionResult> UploadDocumentos()
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _userService.GetUserByUsernameAsync(userName);
            if (user == null)
                return RedirectToAction("Error", "Home");

            var vm = new UploadDocumentoViewModel
            {
                PastaPrincipais = await _pastaService.GetByEmpresaAsync(user.EmpresaContratante),
                SubPastas = await _subPastaService.GetByEmpresaAsync(user.EmpresaContratante)
            };

            ViewBag.Usuario = user.Usuario;
            ViewBag.Empresa = user.EmpresaContratante;
            ViewBag.IsAdmin = User.HasClaim("PermissionAccount", "admin");
            return View(vm);
        }

        [HttpPost]
        public async Task<JsonResult> UploadDocumentos([FromForm] UploadDocumentoViewModel vm)
        {
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var user = await _userService.GetUserByUsernameAsync(userName);
            if (user == null)
                return Json(new { success = false, message = "Usuário não encontrado." });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToArray();
                return Json(new { success = false, message = "Erro de validação.", errors });
            }

            if (vm.NivelSeguranca != "Publico" && string.IsNullOrWhiteSpace(vm.TokenSeguranca))
            {
                return Json(new
                {
                    success = false,
                    message = "Token de segurança obrigatório para nível Restrito ou Confidencial.",
                    errors = new[] { "Token de Segurança é obrigatório." }
                });
            }

            if (vm.NivelSeguranca == "Confidencial" &&
                !User.HasClaim("PermissionAccount", "admin"))
            {
                return Json(new
                {
                    success = false,
                    message = "Apenas admins podem criar documentos confidenciais."
                });
            }

            try
            {
                var pasta = await _pastaService.GetByIdAsync(vm.SelectedPastaPrincipalId);
                var sub = await _subPastaService.GetByIdAsync(vm.SelectedSubPastaId);

                if (pasta == null || sub == null ||
                    pasta.EmpresaContratante != user.EmpresaContratante ||
                    sub.EmpresaContratante != user.EmpresaContratante)
                {
                    return Json(new { success = false, message = "Categoria ou subcategoria inválida." });
                }

                var empresaPrefix = user.EmpresaContratante.Substring(0, 3).ToUpper();
                var pastaPrefix = pasta.NomePastaPrincipal.Substring(0, 3).ToUpper();
                var datePrefix = DateTime.UtcNow.ToString("ddMMyyyy");
                var subPrefix = sub.NomeSubPasta.Substring(0, 3).ToUpper();
                var nomeOriginal = Path.GetFileNameWithoutExtension(vm.Arquivo.FileName);

                var ultimoNome = nomeOriginal.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? nomeOriginal;
                var ext = Path.GetExtension(vm.Arquivo.FileName);
                var name = $"{empresaPrefix}_{pastaPrefix}{datePrefix}{subPrefix}_{ultimoNome}{ext}";

                // ✅ Verificação de duplicidade (sem considerar a data)
                var baseNameWithoutDate = Regex.Replace(name, @"\d{8}", ""); // Remove a data de 8 dígitos
                var baseNameWithoutDateAndExtension = Path.GetFileNameWithoutExtension(baseNameWithoutDate);
                var extension = Path.GetExtension(name);

                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                bool blobExists = false;

                await foreach (var blob in containerClient.GetBlobsAsync())
                {
                    var blobName = blob.Name;
                    var blobBaseName = Path.GetFileNameWithoutExtension(blobName);
                    var blobExt = Path.GetExtension(blobName);

                    if (blobBaseName == baseNameWithoutDateAndExtension && blobExt == extension)
                    {
                        blobExists = true;
                        break;
                    }
                }

                if (blobExists)
                {
                    return Json(new 
                    { 
                        success = false, 
                        message = "Já existe um documento com esse nome. Por favor, renomeie o arquivo e tente novamente." 
                    });
                }

                using var ms = new MemoryStream();
                await vm.Arquivo.CopyToAsync(ms);
                ms.Position = 0;

                var dto = new DocumentoDto
                {
                    Arquivo = new FormFile(ms, 0, ms.Length, vm.Arquivo.Name, name)
                    {
                        Headers = vm.Arquivo.Headers,
                        ContentType = vm.Arquivo.ContentType
                    },
                    NomePastaPrincipal = pasta.NomePastaPrincipal,
                    NomeSubPasta = sub.NomeSubPasta,
                    NivelSeguranca = vm.NivelSeguranca,
                    TokenSeguranca = vm.TokenSeguranca,
                    Descrição = vm.Descricao,
                    Usuario = user.Usuario,
                    EmpresaContratante = user.EmpresaContratante
                };

                await _documentoService.CreateAsync(dto);
                return Json(new { success = true, message = "Upload realizado com sucesso!" });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("BlobAlreadyExists") || ex.Message.Contains("already exists"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Já existe um documento com esse nome. Por favor, renomeie o arquivo e tente novamente."
                    });
                }

                return Json(new { success = false, message = "Erro ao fazer upload: " + ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<JsonResult> GetSubPastasByPastaPrincipal(string nomePastaPrincipal)
        {
            var subPastas = await _subPastaService.GetSubChildrenAsync(nomePastaPrincipal);
            var result = subPastas.Select(s => new { s.Id, s.NomeSubPasta }).ToList();
            return Json(result);
        }
    }
}