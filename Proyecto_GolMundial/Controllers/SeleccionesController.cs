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
            await SincronizarEliminaciones();
            return await _context.Selecciones.Include(s => s.Grupo).ToListAsync();
        }

        private async Task SincronizarEliminaciones()
        {
            try
            {
                var selecciones = await _context.Selecciones.ToListAsync();
                var partidos = await _context.Partidos.Where(p => p.Estado == "FINALIZADO").ToListAsync();

                var eliminadosIds = new HashSet<int>();

                var grupos = selecciones.Select(s => s.GrupoId).Where(g => !string.IsNullOrEmpty(g)).Distinct().ToList();
                foreach (var g in grupos)
                {
                    var equiposDelGrupo = selecciones.Where(s => s.GrupoId == g).ToList();
                    var partidosDelGrupo = partidos.Where(p => 
                        (p.FaseCodigo == "GRUPOS" || p.FaseCodigo.Contains("GRUPO", StringComparison.OrdinalIgnoreCase)) &&
                        (p.GrupoCodigo == g || equiposDelGrupo.Any(e => e.Id == p.EquipoLocalId || e.Id == p.EquipoVisitanteId))
                    ).ToList();

                    var ranking = equiposDelGrupo.Select(e => {
                        var misPartidos = partidosDelGrupo.Where(p => p.EquipoLocalId == e.Id || p.EquipoVisitanteId == e.Id).ToList();
                        int pj = misPartidos.Count;
                        int pts = misPartidos.Sum(p => p.EquipoLocalId == e.Id ? (p.GolesLocal > p.GolesVisitante ? 3 : p.GolesLocal == p.GolesVisitante ? 1 : 0) : (p.GolesVisitante > p.GolesLocal ? 3 : p.GolesVisitante == p.GolesLocal ? 1 : 0));
                        int gf = misPartidos.Sum(p => p.EquipoLocalId == e.Id ? (p.GolesLocal ?? 0) : (p.GolesVisitante ?? 0));
                        int gc = misPartidos.Sum(p => p.EquipoLocalId == e.Id ? (p.GolesVisitante ?? 0) : (p.GolesLocal ?? 0));
                        return new { Equipo = e, PartidosJugados = pj, Puntos = pts, GolesFavor = gf, GolesContra = gc, DiferenciaGoles = gf - gc };
                    }).OrderByDescending(x => x.Puntos)
                      .ThenByDescending(x => x.DiferenciaGoles)
                      .ThenByDescending(x => x.GolesFavor)
                      .ToList();

                    bool grupoCompleto = partidosDelGrupo.Count >= 6 || (ranking.Count > 0 && ranking.All(x => x.PartidosJugados >= 3));

                    for (int i = 0; i < ranking.Count; i++)
                    {
                        var item = ranking[i];
                        if (i >= 2)
                        {
                            if (grupoCompleto || item.PartidosJugados >= 3)
                            {
                                eliminadosIds.Add(item.Equipo.Id);
                            }
                        }
                    }
                }

                foreach (var p in partidos)
                {
                    if (p.FaseCodigo != "GRUPOS" && !p.FaseCodigo.Contains("GRUPO", StringComparison.OrdinalIgnoreCase) && !p.FaseCodigo.Contains("TERCER", StringComparison.OrdinalIgnoreCase))
                    {
                        if (p.GolesLocal > p.GolesVisitante) eliminadosIds.Add(p.EquipoVisitanteId);
                        else if (p.GolesVisitante > p.GolesLocal) eliminadosIds.Add(p.EquipoLocalId);
                    }
                }

                bool cambio = false;
                foreach (var s in selecciones)
                {
                    bool debedeEstarEliminado = eliminadosIds.Contains(s.Id);
                    if (s.Eliminado != debedeEstarEliminado)
                    {
                        s.Eliminado = debedeEstarEliminado;
                        _context.Entry(s).State = EntityState.Modified;
                        cambio = true;
                    }
                }

                if (cambio)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch { }
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
