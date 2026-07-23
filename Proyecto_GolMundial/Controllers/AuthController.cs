using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Proyecto_GolMundial.Data;
using Proyecto_GolMundial.DTOs.Auth;
using Proyecto_GolMundial.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Proyecto_GolMundial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EstadisticasDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(EstadisticasDbContext context, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Usuarios.Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized(new { message = "Credenciales incorrectas" });
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest(new { message = "El nombre de usuario ya existe" });
            }

            var newUser = new Usuario
            {
                Username = request.Username,
                Nombre = request.Nombre,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RolId = 2 // USUARIO por defecto
            };

            _context.Usuarios.Add(newUser);
            await _context.SaveChangesAsync();

            // Notificar al backend UTNGolCoin (Java) para crear la billetera con el bono de bienvenida (10 GC)
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Forzar un int64 para que coincida con el Long de Java
                var billeteraPayload = new { usuarioId = (long)newUser.Id };
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(billeteraPayload), Encoding.UTF8, "application/json");
                
                var response = await client.PostAsync("http://192.168.0.15:8080/utngolcoin-backend/api/billeteras", jsonContent);
                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error al crear billetera en Java: {response.StatusCode} - {errorResponse}");
                    // Aunque falle la billetera, el usuario ya se creó. Podríamos retornar un warning.
                }
                else 
                {
                    Console.WriteLine($"Billetera creada exitosamente en Java para usuario: {newUser.Id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Excepción al crear billetera en Java: {ex.Message}");
            }

            var token = GenerateJwtToken(newUser);
            return Ok(new { token });
        }

        private string GenerateJwtToken(Usuario user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Rol?.Nombre ?? "USUARIO")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
