// VoxDocs.Controllers.PagamentoConcluidoController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxDocs.Data;
using VoxDocs.Models;

[ApiController]
[Route("api/[controller]")]
public class PagamentoConcluidoController : ControllerBase
{
    private readonly VoxDocsContext _context;

    public PagamentoConcluidoController(VoxDocsContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PagamentoConcluido>>> GetPagamentos()
    {
        return await _context.PagamentosConcluidos.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PagamentoConcluido>> GetPagamento(int id)
    {
        var pagamento = await _context.PagamentosConcluidos.FindAsync(id);

        if (pagamento == null)
        {
            return NotFound();
        }

        return pagamento;
    }
}