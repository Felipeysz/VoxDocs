using Microsoft.AspNetCore.Mvc;
using VoxDocs.Models;
using VoxDocs.Services;
using VoxDocs.DTO;
using VoxDocs.BusinessRules;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace VoxDocs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosController : ControllerBase
    {
        private readonly IDocumentoService _service;
        private readonly AzureBlobService _blobService;
        private readonly DocumentoBusinessRules _rules;

        public DocumentosController(
            IDocumentoService service,
            AzureBlobService blobService,
            DocumentoBusinessRules rules)
        {
            _service = service;
            _blobService = blobService;
            _rules = rules;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] DocumentoDto dto)
        {
            try
            {
                await _rules.ValidateDocumentoUploadAsync(dto);

                // Chama a Service para criar o documento
                var createdDoc = await _service.CreateAsync(dto);
                return Ok(createdDoc);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log do erro para depuração
                Console.WriteLine($"Erro ao realizar o upload: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { message = "Erro ao realizar o upload", detalhes = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, [FromQuery] string token = null)
        {
            try
            {
                var doc = await _service.GetByIdAsync(id, token);
                await _service.IncrementarAcessoAsync(id);
                return Ok(doc);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno no servidor", detalhes = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, [FromQuery] string token = null)
        {
            try
            {
                await _service.DeleteAsync(id, token);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno no servidor", detalhes = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var docs = await _service.GetAllAsync();
                return Ok(docs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno no servidor", detalhes = ex.Message });
            }
        }

        [HttpGet("acessos/{id}/{dias}")]
        public async Task<IActionResult> GetAcessosDocumento(int id, int dias)
        {
            try
            {
                var acessos = await _service.GetAcessosDocumentoAsync(id, dias);
                if (acessos == null) return NotFound();
                return Ok(acessos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno no servidor", detalhes = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocumento(int id, [FromForm] DocumentoUpdateDto updateDto,[FromForm] string? tokenSeguranca)
        {
            try
            {
                updateDto.Id = id;
                // Validação com token separado
                await _rules.ValidateDocumentoUpdateAsync(updateDto, tokenSeguranca);
                
                var updatedDoc = await _service.UpdateAsync(updateDto);
                return Ok(updatedDoc);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("download/{nomeArquivo}")]
        public async Task<IActionResult> DownloadDocumento(string nomeArquivo, [FromQuery] string token = null)
        {
            try
            {
                var (stream, contentType) = await _service.DownloadDocumentoProtegidoAsync(nomeArquivo, token);
                return File(stream, contentType, nomeArquivo);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro ao baixar o documento", detalhes = ex.Message });
            }
        }

        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken([FromQuery] string nomeArquivo, [FromQuery] string token)
        {
            try
            {
                // Chama a service para validar o token
                var valido = await _service.ValidateTokenDocumentoAsync(nomeArquivo, token);

                // Se quiser retornar 200 mesmo quando inválido, apenas muda o corpo
                return Ok(new { sucesso = valido });
            }
            catch (UnauthorizedAccessException ex)
            {
                // 401 Unauthorized com corpo detalhado
                return Unauthorized(new { sucesso = false, mensagem = ex.Message });
            }
            catch (ArgumentException ex)
            {
                // 404 Not Found (nomeArquivo inválido, etc)
                return NotFound(new { sucesso = false, mensagem = ex.Message });
            }
            catch (Exception ex)
            {
                // 500 Internal Server Error
                return StatusCode(500, new { sucesso = false, mensagem = "Erro ao validar token", detalhes = ex.Message });
            }
        }
    }
}