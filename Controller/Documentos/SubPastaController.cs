using Microsoft.AspNetCore.Mvc;
using VoxDocs.DTO;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubPastaController : ControllerBase
    {
        private readonly ISubPastaService _service;

        public SubPastaController(ISubPastaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DTOSubPasta>>> GetAll()
        {
            var subPastas = await _service.GetAllAsync();
            return Ok(subPastas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DTOSubPasta>> GetById(int id)
        {
            var subPasta = await _service.GetByIdAsync(id);
            if (subPasta == null) return NotFound();
            return Ok(subPasta);
        }

        [HttpPost]
        public async Task<ActionResult<DTOSubPasta>> Create(DTOSubPastaCreate dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();

        // Nova rota para buscar subpastas pelo nome da PastaPrincipal
        [HttpGet("subchildren/{nomePastaPrincipal}")]
        public async Task<ActionResult<IEnumerable<DTOSubPasta>>> GetSubChildren(string nomePastaPrincipal)
        {
            var subPastas = await _service.GetSubChildrenAsync(nomePastaPrincipal);
            return Ok(subPastas);
        }
    }
}