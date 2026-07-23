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

        // NUEVO ENDPOINT PARA UTNGolCoin (Devuelve solo la fecha como string ISO)
        // GET: api/Partidos/5/fecha
        [HttpGet("{id}/fecha")]
        public async Task<ActionResult<string>> GetPartidoFecha(int id)
        {
            var partido = await _context.Partidos.FindAsync(id);

            if (partido == null)
            {
                return NotFound();
            }

            // Devuelve la fecha en el formato esperado por Java como texto plano
            return Content(partido.FechaHoraUtc.ToString("s"), "text/plain");
        }

        // GET: api/Partidos/grupo/A
        [HttpGet("grupo/{codigo}")]
        public async Task<ActionResult<IEnumerable<Partido>>> GetPartidosPorGrupo(string codigo)
        {
            var partidos = await _context.Partidos
                .Include(p => p.EquipoLocal)
                .Include(p => p.EquipoVisitante)
                .Include(p => p.Sede)
                .Include(p => p.Fase)
                .Where(p => p.GrupoCodigo == codigo)
                .ToListAsync();

            return Ok(partidos);
        }

        // POST: api/Partidos
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<Partido>> PostPartido([FromBody] Partido partido)
        {
            if (partido == null) return BadRequest();

            // Validación de equipos eliminados
            var local = await _context.Selecciones.FindAsync(partido.EquipoLocalId);
            var visitante = await _context.Selecciones.FindAsync(partido.EquipoVisitanteId);
            if ((local != null && local.Eliminado) || (visitante != null && visitante.Eliminado))
            {
                return BadRequest("No se puede programar el partido porque uno de los equipos ya está eliminado del Mundial.");
            }

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

            // Validación de equipos eliminados
            var local = await _context.Selecciones.FindAsync(partido.EquipoLocalId);
            var visitante = await _context.Selecciones.FindAsync(partido.EquipoVisitanteId);
            if ((local != null && local.Eliminado) || (visitante != null && visitante.Eliminado))
            {
                return BadRequest("No se puede actualizar el partido porque uno de los equipos ya está eliminado del Mundial.");
            }

            _context.Entry(partido).State = EntityState.Modified;

            try
            {
                if (partido.Estado == "FINALIZADO")
                {
                    await CalcularEliminacionAutomatica(partido);
                }
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

            // Ejecutar la lógica de eliminación automática
            await CalcularEliminacionAutomatica(partido);

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

        private async Task CalcularEliminacionAutomatica(Partido partido)
        {
            // Solo procesar si el partido ha finalizado
            if (partido.Estado != "FINALIZADO") return;

            bool esFaseGrupos = partido.FaseCodigo == "GRUPOS" || partido.FaseCodigo.Contains("GRUPO", StringComparison.OrdinalIgnoreCase);

            if (esFaseGrupos)
            {
                var grupoCod = partido.GrupoCodigo;
                if (string.IsNullOrEmpty(grupoCod) && partido.EquipoLocalId > 0)
                {
                    var localEquipo = await _context.Selecciones.FindAsync(partido.EquipoLocalId);
                    if (localEquipo != null) grupoCod = localEquipo.GrupoId;
                }

                if (!string.IsNullOrEmpty(grupoCod))
                {
                    var partidosDelGrupo = await _context.Partidos
                        .Where(p => p.GrupoCodigo == grupoCod)
                        .ToListAsync();

                    var equipos = await _context.Selecciones.Where(s => s.GrupoId == grupoCod).ToListAsync();
                    var partidosFinalizados = partidosDelGrupo.Where(p => p.Estado == "FINALIZADO").ToList();

                    var estadisticas = equipos.Select(e => new {
                        Equipo = e,
                        PartidosJugados = partidosFinalizados.Count(p => p.EquipoLocalId == e.Id || p.EquipoVisitanteId == e.Id),
                        Puntos = partidosFinalizados.Sum(p => 
                            p.EquipoLocalId == e.Id ? (p.GolesLocal > p.GolesVisitante ? 3 : p.GolesLocal == p.GolesVisitante ? 1 : 0) :
                            p.EquipoVisitanteId == e.Id ? (p.GolesVisitante > p.GolesLocal ? 3 : p.GolesVisitante == p.GolesLocal ? 1 : 0) : 0),
                        DiferenciaGoles = partidosFinalizados.Sum(p =>
                            p.EquipoLocalId == e.Id ? ((p.GolesLocal ?? 0) - (p.GolesVisitante ?? 0)) :
                            p.EquipoVisitanteId == e.Id ? ((p.GolesVisitante ?? 0) - (p.GolesLocal ?? 0)) : 0)
                    }).OrderByDescending(x => x.Puntos).ThenByDescending(x => x.DiferenciaGoles).ToList();

                    bool todosFinalizados = partidosDelGrupo.Count > 0 && partidosDelGrupo.All(p => p.Estado == "FINALIZADO");

                    if (todosFinalizados)
                    {
                        // En fase de grupos califican los 2 primeros. El 3er y 4to quedan eliminados.
                        var eliminados = estadisticas.Skip(2).Select(x => x.Equipo).ToList();
                        foreach (var eq in eliminados)
                        {
                            eq.Eliminado = true;
                            _context.Entry(eq).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        // Si un equipo ya jugó todos sus partidos de grupo (>= 3) y quedó en posición 3 o 4
                        foreach (var item in estadisticas)
                        {
                            if (item.PartidosJugados >= 3)
                            {
                                int pos = estadisticas.FindIndex(x => x.Equipo.Id == item.Equipo.Id);
                                if (pos >= 2)
                                {
                                    item.Equipo.Eliminado = true;
                                    _context.Entry(item.Equipo).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                }
            }
            // Fases eliminatorias directas (Octavos, Cuartos, Semifinal, Final, Dieciseisavos, etc.)
            else if (!partido.FaseCodigo.Contains("TERCER", StringComparison.OrdinalIgnoreCase))
            {
                if (partido.GolesLocal > partido.GolesVisitante)
                {
                    var visitante = await _context.Selecciones.FindAsync(partido.EquipoVisitanteId);
                    if (visitante != null)
                    {
                        visitante.Eliminado = true;
                        _context.Entry(visitante).State = EntityState.Modified;
                    }
                }
                else if (partido.GolesVisitante > partido.GolesLocal)
                {
                    var local = await _context.Selecciones.FindAsync(partido.EquipoLocalId);
                    if (local != null)
                    {
                        local.Eliminado = true;
                        _context.Entry(local).State = EntityState.Modified;
                    }
                }
            }
        }
    }
}
