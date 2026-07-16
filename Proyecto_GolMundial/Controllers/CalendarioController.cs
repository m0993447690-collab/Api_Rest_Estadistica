using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Data;
using Proyecto_GolMundial.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Proyecto_GolMundial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarioController : ControllerBase
    {
        private readonly EstadisticasDbContext _context;

        public CalendarioController(EstadisticasDbContext context)
        {
            _context = context;
        }

        // GET: api/Calendario
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Calendario>>> GetCalendario()
        {
            return await _context.Calendarios
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.EquipoLocal)
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.EquipoVisitante)
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.Sede)
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.Fase)
                .ToListAsync();
        }

        // GET: api/Calendario/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Calendario>> GetCalendarioItem(int id)
        {
            var item = await _context.Calendarios
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.EquipoLocal)
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.EquipoVisitante)
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.Sede)
                .Include(c => c.Partido)
                    .ThenInclude(p => p!.Fase)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        // POST: api/Calendario
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<Calendario>> PostCalendario([FromBody] Calendario calendario)
        {
            if (calendario == null) return BadRequest();
            _context.Calendarios.Add(calendario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCalendarioItem), new { id = calendario.Id }, calendario);
        }

        // PUT: api/Calendario/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> PutCalendario(int id, [FromBody] Calendario calendario)
        {
            if (id != calendario.Id) return BadRequest();

            _context.Entry(calendario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CalendarioExists(id))
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

        // DELETE: api/Calendario/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteCalendario(int id)
        {
            var item = await _context.Calendarios.FindAsync(id);
            if (item == null) return NotFound();

            _context.Calendarios.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CalendarioExists(int id)
        {
            return _context.Calendarios.Any(e => e.Id == id);
        }
    }
}

