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

            return seleccion;
        }
    }
}
