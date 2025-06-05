using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VoxDocs.DTO;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagamentoFalsoController : ControllerBase
    {
        private readonly IPagamentoCartaoFalsoService _cartaoService;
        private readonly IPagamentoPixFalsoService _pixService;

        public PagamentoFalsoController(
            IPagamentoCartaoFalsoService cartaoService,
            IPagamentoPixFalsoService pixService)
        {
            _cartaoService = cartaoService;
            _pixService = pixService;
        }

        [HttpPost("cartao")]
        public async Task<IActionResult> Cartao([FromBody] PagamentoCartaoRequestDto dto) 
        {
            try
            {
                var msg = await _cartaoService.ProcessarPagamentoCartaoFalsoAsync(dto);
                return Ok(new { Mensagem = msg });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpPost("pix/gerar")]
        public async Task<IActionResult> GerarPix([FromBody] PagamentoPixRequestDto dto) // DTO atualizado
        {
            try
            {
                var (pagamentoPixId, qrCodeUrl) = await _pixService.GerarPixAsync(dto);
                return Ok(new
                {
                    pagamentoPixId,
                    qrCode = qrCodeUrl,
                    mensagem = "Pagamento Pix gerado e confirmado com sucesso!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }
    }
}