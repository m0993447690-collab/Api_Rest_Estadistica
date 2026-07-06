using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Data;
using Proyecto_GolMundial.DTOs.Grupo;

namespace Proyecto_GolMundial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GruposController : ControllerBase
    {
        private readonly EstadisticasDbContext _context;

        public GruposController(EstadisticasDbContext context)
        {
            _context = context;
        }

        // GET: api/Grupos
        [HttpGet]
        public async Task<IActionResult> GetGrupos()
        {
            var grupos = await _context.Grupos.ToListAsync();
            return Ok(grupos);
        }

        // GET: api/Grupos/5/Posiciones
        [HttpGet("{codigo}/posiciones")]
        public async Task<IActionResult> GetPosiciones(string codigo)
        {
            var grupo = await _context.Grupos.FindAsync(codigo);
            if (grupo == null) return NotFound();

            var selecciones = await _context.Selecciones.Where(s => s.GrupoId == codigo).ToListAsync();
            var partidos = await _context.Partidos
                .Where(p => p.GrupoCodigo == codigo && p.Estado == "FINALIZADO")
                .ToListAsync();

            var posiciones = new PosicionesResponse
            {
                GrupoCodigo = grupo.Codigo,
                GrupoNombre = grupo.Nombre
            };

            foreach (var seleccion in selecciones)
            {
                var stats = new PosicionEquipo { EquipoId = seleccion.Id, Nombre = seleccion.Nombre };

                foreach (var partido in partidos)
                {
                    if (partido.EquipoLocalId == seleccion.Id)
                    {
                        stats.PartidosJugados++;
                        stats.GolesAFavor += partido.GolesLocal ?? 0;
                        stats.GolesEnContra += partido.GolesVisitante ?? 0;

                        if (partido.GolesLocal > partido.GolesVisitante) stats.PartidosGanados++;
                        else if (partido.GolesLocal == partido.GolesVisitante) stats.PartidosEmpatados++;
                        else stats.PartidosPerdidos++;
                    }
                    else if (partido.EquipoVisitanteId == seleccion.Id)
                    {
                        stats.PartidosJugados++;
                        stats.GolesAFavor += partido.GolesVisitante ?? 0;
                        stats.GolesEnContra += partido.GolesLocal ?? 0;

                        if (partido.GolesVisitante > partido.GolesLocal) stats.PartidosGanados++;
                        else if (partido.GolesVisitante == partido.GolesLocal) stats.PartidosEmpatados++;
                        else stats.PartidosPerdidos++;
                    }
                }

                stats.DiferenciaGoles = stats.GolesAFavor - stats.GolesEnContra;
                stats.Puntos = (stats.PartidosGanados * 3) + stats.PartidosEmpatados;

                posiciones.Posiciones.Add(stats);
            }

            posiciones.Posiciones = posiciones.Posiciones
                .OrderByDescending(p => p.Puntos)
                .ThenByDescending(p => p.DiferenciaGoles)
                .ThenByDescending(p => p.GolesAFavor)
                .ToList();

            return Ok(posiciones);
        }
    }
}
