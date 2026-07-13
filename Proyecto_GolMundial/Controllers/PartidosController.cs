using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Data;
using Proyecto_GolMundial.DTOs.Partido;
using Proyecto_GolMundial.Models;
using Proyecto_GolMundial.Services;

namespace Proyecto_GolMundial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidosController : ControllerBase
    {
        private readonly EstadisticasDbContext _context;
        private readonly UtnGolCoinClient _utnGolCoinClient;

        public PartidosController(EstadisticasDbContext context, UtnGolCoinClient utnGolCoinClient)
        {
            _context = context;
            _utnGolCoinClient = utnGolCoinClient;
        }

        // GET: api/Partidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Partido>>> GetPartidos()
        {
            return await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Sede)
                .Include(p => p.Fase)
                .ToListAsync();
        }

        // GET: api/Partidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Partido>> GetPartido(int id)
        {
            var partido = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Sede)
                .Include(p => p.Fase)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (partido == null)
            {
                return NotFound();
            }

            return Ok(partido);
        }

        // POST: api/Partidos
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<Partido>> PostPartido([FromBody] Partido partido)
        {
            if (partido == null) return BadRequest();
            _context.Partidos.Add(partido);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPartido), new { id = partido.Id }, partido);
        }

        // PUT: api/Partidos/5
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> PutPartido(int id, [FromBody] Partido partido)
        {
            if (id != partido.Id) return BadRequest();

            _context.Entry(partido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartidoExists(id))
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

        // PUT: api/Partidos/5/resultado
        [HttpPut("{id}/resultado")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> UpdateResultado(int id, [FromBody] UpdatePartidoRequest request)
        {
            var partido = await _context.Partidos.FindAsync(id);

            if (partido == null)
            {
                return NotFound();
            }

            partido.GolesLocal = request.GolesLocal;
            partido.GolesVisitante = request.GolesVisitante;
            partido.Estado = "FINALIZADO";

            _context.Entry(partido).State = EntityState.Modified;

            // Determinar resultado oficial para UTNGolCoin (1: Local, X: Empate, 2: Visitante)
            string resultadoOficial = "X";
            if (partido.GolesLocal > partido.GolesVisitante) resultadoOficial = "1";
            else if (partido.GolesVisitante > partido.GolesLocal) resultadoOficial = "2";

            // En un caso real las cuotas pueden ser dinámicas. Aquí simulamos una básica.
            int cuota = 2; 

            await _context.SaveChangesAsync();

            // Notificar a la API de UTNGolCoin (Java)
            await _utnGolCoinClient.NotificarResultadoLiquidacion(partido.Id, resultadoOficial, cuota);

            return NoContent();
        }

        // DELETE: api/Partidos/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<IActionResult> DeletePartido(int id)
        {
            var partido = await _context.Partidos.FindAsync(id);
            if (partido == null) return NotFound();

            _context.Partidos.Remove(partido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PartidoExists(int id)
        {
            return _context.Partidos.Any(e => e.Id == id);
        }
    }
}
