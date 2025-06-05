using Microsoft.AspNetCore.Mvc;
using VoxDocs.DTO;
using VoxDocs.Services;

namespace VoxDocs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PastaPrincipalController : ControllerBase
    {
        private readonly IPastaPrincipalService _service;

        public PastaPrincipalController(IPastaPrincipalService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DTOPastaPrincipal>>> GetAll()
        {
            var pastas = await _service.GetAllAsync();
            return Ok(pastas);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DTOPastaPrincipal>> GetById(int id)
        {
            var pasta = await _service.GetByIdAsync(id);
            if (pasta == null) return NotFound();
            return Ok(pasta);
        }

        [HttpPost]
        public async Task<ActionResult<DTOPastaPrincipal>> Create(DTOPastaPrincipalCreate dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
            => await _service.DeleteAsync(id) ? NoContent() : NotFound();
    }
}