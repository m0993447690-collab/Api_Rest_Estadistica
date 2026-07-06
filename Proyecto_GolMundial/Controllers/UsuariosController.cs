using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Data;
using Proyecto_GolMundial.Models;

namespace Proyecto_GolMundial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMINISTRADOR")]
    public class UsuariosController : ControllerBase
    {
        private readonly EstadisticasDbContext _context;

        public UsuariosController(EstadisticasDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            // Retornar lista de usuarios excluyendo las contraseñas
            return await _context.Usuarios
                .Include(u => u.Rol)
                .Select(u => new Usuario
                {
                    Id = u.Id,
                    Username = u.Username,
                    Nombre = u.Nombre,
                    RolId = u.RolId,
                    Rol = u.Rol,
                    Password = null // Ocultar por seguridad
                })
                .ToListAsync();
        }

        // PUT: api/Usuarios/5/rol/1
        [HttpPut("{id}/rol/{rolId}")]
        public async Task<IActionResult> UpdateUserRole(int id, int rolId)
        {
            var user = await _context.Usuarios.FindAsync(id);
            if (user == null) return NotFound();

            var rol = await _context.Roles.FindAsync(rolId);
            if (rol == null) return BadRequest("El rol especificado no existe.");

            user.RolId = rolId;
            _context.Entry(user).State = EntityState.Modified;
            
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
