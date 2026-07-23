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
                    Password = null! // Ocultar por seguridad
                })
                .ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Select(u => new Usuario
                {
                    Id = u.Id,
                    Username = u.Username,
                    Nombre = u.Nombre,
                    RolId = u.RolId,
                    Rol = u.Rol,
                    Password = null! // Ocultar por seguridad
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return Ok(usuario);
        }

        // POST: api/Usuarios
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario([FromBody] Usuario usuario)
        {
            if (usuario == null) return BadRequest();
            if (string.IsNullOrEmpty(usuario.Password)) return BadRequest("La contraseña es requerida.");

            // Verificar si el usuario ya existe
            if (await _context.Usuarios.AnyAsync(u => u.Username == usuario.Username))
            {
                return Conflict(new { message = "El nombre de usuario ya existe." });
            }

            // Cifrar la contraseña
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Limpiar contraseña de la respuesta
            usuario.Password = null!;

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] Usuario usuario)
        {
            if (id != usuario.Id) return BadRequest();

            var dbUser = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (dbUser == null) return NotFound();

            // Si el password en el request está vacío, mantenemos el password existente.
            if (string.IsNullOrEmpty(usuario.Password))
            {
                usuario.Password = dbUser.Password;
            }
            else
            {
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
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

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}
