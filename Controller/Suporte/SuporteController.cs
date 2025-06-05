using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VoxDocs.DTO;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuporteVoxDocsController : ControllerBase
    {
        private readonly ISuporteVoxDocsService _suporteService;

        public SuporteVoxDocsController(ISuporteVoxDocsService suporteService)
        {
            _suporteService = suporteService;
        }

        /// POST: api/SuporteVoxDocs/abrir
        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirChamado([FromBody] DTOAbrirChamado dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var chamado = await _suporteService.AbrirChamadoAsync(dto);
                return CreatedAtAction(nameof(ObterChamadoPorId), new { chamadoId = chamado.Id }, chamado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        /// GET: api/SuporteVoxDocs
        [HttpGet]
        public async Task<IActionResult> ObterTodosChamados()
        {
            var lista = await _suporteService.ObterTodosChamadosAsync();
            return Ok(lista);
        }

        /// GET: api/SuporteVoxDocs/abertos
        [HttpGet("abertos")]
        public async Task<IActionResult> ObterChamadosAbertos()
        {
            var lista = await _suporteService.ObterChamadosAbertosAsync();
            return Ok(lista);
        }


        /// GET: api/SuporteVoxDocs/{chamadoId}
        [HttpGet("{chamadoId}")]
        public async Task<IActionResult> ObterChamadoPorId([FromRoute] int chamadoId)
        {
            try
            {
                var chamado = await _suporteService.ObterChamadoPorIdAsync(chamadoId);
                return Ok(chamado);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Chamado com ID {chamadoId} não encontrado.");
            }
        }


        /// POST: api/SuporteVoxDocs/responder
        [HttpPost("responder")]
        public async Task<IActionResult> ResponderChamado([FromBody] DTOResponderChamado dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var mensagem = await _suporteService.ResponderChamadoAsync(dto);
                return Ok(mensagem);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// POST: api/SuporteVoxDocs/finalizar
        [HttpPost("finalizar")]
        public async Task<IActionResult> FinalizarChamado([FromBody] DTOFinalizarChamado dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var chamado = await _suporteService.FinalizarChamadoAsync(dto);
                return Ok(chamado);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// PATCH: api/SuporteVoxDocs/arquivar/{chamadoId}
        [HttpPatch("arquivar/{chamadoId}")]
        public async Task<IActionResult> ArquivarChamado([FromRoute] int chamadoId)
        {
            try
            {
                var chamado = await _suporteService.ArquivarChamadoAsync(chamadoId);
                return Ok(chamado);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (InvalidOperationException inv)
            {
                return BadRequest(inv.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// PATCH: api/SuporteVoxDocs/reabrir/{chamadoId}
        [HttpPatch("reabrir/{chamadoId}")]
        public async Task<IActionResult> ReabrirChamado([FromRoute] int chamadoId)
        {
            try
            {
                var chamado = await _suporteService.ReabrirChamadoAsync(chamadoId);
                return Ok(chamado);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(knf.Message);
            }
            catch (InvalidOperationException inv)
            {
                return BadRequest(inv.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// DELETE: api/SuporteVoxDocs/{chamadoId}
        [HttpDelete("{chamadoId}")]
        public async Task<IActionResult> DeletarChamado([FromRoute] int chamadoId)
        {
            try
            {
                var sucesso = await _suporteService.DeletarChamadoAsync(chamadoId);
                if (!sucesso)
                    return NotFound($"Chamado com ID {chamadoId} não existe.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
