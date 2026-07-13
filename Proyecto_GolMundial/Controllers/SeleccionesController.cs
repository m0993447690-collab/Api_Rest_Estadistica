using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Data;
using Proyecto_GolMundial.Models;

namespace Proyecto_GolMundial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeleccionesController : ControllerBase
    {
        private readonly EstadisticasDbContext _context;

        public SeleccionesController(EstadisticasDbContext context)
        {
            _context = context;
        }

        // GET: api/Selecciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Seleccion>>> GetSelecciones()
        {
            return await _context.Selecciones.Include(s => s.Grupo).ToListAsync();
        }

        // GET: api/Selecciones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Seleccion>> GetSeleccion(int id)
        {
            var seleccion = await _context.Selecciones
                .Include(s => s.Grupo)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (seleccion == null)
            {
                return NotFound();
            }

            return Ok(seleccion);
        }

        // POST: api/Selecciones
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<Seleccion>> PostSeleccion([FromBody] Seleccion seleccion)
        {
            if (seleccion == null) return BadRequest();
            _context.Selecciones.Add(seleccion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSeleccion), new { id = seleccion.Id }, seleccion);
        }

        // PUT: api/Selecciones/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> PutSeleccion(int id, [FromBody] Seleccion seleccion)
        {
            if (id != seleccion.Id) return BadRequest();

            _context.Entry(seleccion).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeleccionExists(id))
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

        // DELETE: api/Selecciones/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeleteSeleccion(int id)
        {
            var seleccion = await _context.Selecciones.FindAsync(id);
            if (seleccion == null) return NotFound();

            _context.Selecciones.Remove(seleccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SeleccionExists(int id)
        {
            return _context.Selecciones.Any(e => e.Id == id);
        }
    }
}
