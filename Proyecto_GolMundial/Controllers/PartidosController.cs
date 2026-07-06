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
    }
}
