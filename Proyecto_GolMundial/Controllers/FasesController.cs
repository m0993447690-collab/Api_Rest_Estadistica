using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Data;
using Proyecto_GolMundial.Models;

namespace Proyecto_GolMundial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FasesController : ControllerBase
    {
        private readonly EstadisticasDbContext _context;

        public FasesController(EstadisticasDbContext context)
        {
            _context = context;
        }

        // GET: api/Fases
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fase>>> GetFases()
        {
            return await _context.Fases.ToListAsync();
        }

        // GET: api/Fases/5
        [HttpGet("{codigo}")]
        public async Task<ActionResult<Fase>> GetFase(string codigo)
        {
            var fase = await _context.Fases.FindAsync(codigo);

            if (fase == null)
            {
                return NotFound();
            }

            return Ok(fase);
        }

        // POST: api/Fases
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<Fase>> PostFase([FromBody] Fase fase)
        {
            if (fase == null) return BadRequest();
            _context.Fases.Add(fase);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (FaseExists(fase.Codigo))
                {
                    return Conflict(new { message = "La fase ya existe." });
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetFase), new { codigo = fase.Codigo }, fase);
        }

        // PUT: api/Fases/5
        [HttpPut("{codigo}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> PutFase(string codigo, [FromBody] Fase fase)
        {
            if (codigo != fase.Codigo) return BadRequest();

            _context.Entry(fase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FaseExists(codigo))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Fases/5
        [HttpDelete("{codigo}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteFase(string codigo)
        {
            var fase = await _context.Fases.FindAsync(codigo);
            if (fase == null) return NotFound();

            _context.Fases.Remove(fase);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FaseExists(string codigo)
        {
            return _context.Fases.Any(e => e.Codigo == codigo);
        }
    }
}
