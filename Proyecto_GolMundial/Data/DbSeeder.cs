using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Models;

namespace Proyecto_GolMundial.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(EstadisticasDbContext context, string jsonFilePath)
        {
            if (!File.Exists(jsonFilePath))
            {
                throw new FileNotFoundException($"Seed data file not found at: {jsonFilePath}");
            }

            // Ensure calendario table exists
            var createTableSql = @"
CREATE TABLE IF NOT EXISTS calendario (
    id INT PRIMARY KEY AUTO_INCREMENT,
    partido_id INT NOT NULL,
    fecha_hora_local DATETIME NOT NULL,
    zona_horaria VARCHAR(50) NOT NULL,
    observaciones VARCHAR(255),
    FOREIGN KEY (partido_id) REFERENCES partidos(id) ON DELETE CASCADE
);";
            await context.Database.ExecuteSqlRawAsync(createTableSql);

            var jsonText = await File.ReadAllTextAsync(jsonFilePath);

            using var doc = JsonDocument.Parse(jsonText);
            var root = doc.RootElement;

            // 1. Seed Roles
            if (root.TryGetProperty("roles", out var rolesElement))
            {
                foreach (var item in rolesElement.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var nombre = item.GetProperty("nombre").GetString();
                    var descripcion = item.TryGetProperty("descripcion", out var descVal) ? descVal.GetString() : null;

                    if (!await context.Roles.AnyAsync(r => r.Id == id))
                    {
                        context.Roles.Add(new Rol
                        {
                            Id = id,
                            Nombre = nombre ?? string.Empty,
                            Descripcion = descripcion
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // 2. Seed UsuariosIniciales
            if (root.TryGetProperty("usuariosIniciales", out var usuariosElement))
            {
                foreach (var item in usuariosElement.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var username = item.GetProperty("username").GetString();
                    var nombre = item.GetProperty("nombre").GetString();
                    var rolId = item.GetProperty("rolId").GetInt32();
                    
                    if (username != null && !await context.Usuarios.AnyAsync(u => u.Username == username))
                    {
                        // Use default BCrypt hash for admin (corresponds to 'CAMBIAR_EN_PRIMER_USO' or SQL setup)
                        context.Usuarios.Add(new Usuario
                        {
                            Id = id,
                            Username = username,
                            Nombre = nombre ?? string.Empty,
                            RolId = rolId,
                            Password = "$2a$12$R.P96/9G.F4fJvB97X8vCOuYhQ/h4eI5R.UuX7yKqW3lMh.K2Jq7e"
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // 3. Seed Fases
            if (root.TryGetProperty("fases", out var fasesElement))
            {
                foreach (var item in fasesElement.EnumerateArray())
                {
                    var codigo = item.GetProperty("codigo").GetString();
                    var nombre = item.GetProperty("nombre").GetString();
                    var fechas = item.TryGetProperty("fechas", out var fchVal) ? fchVal.GetString() : null;

                    if (codigo != null && !await context.Fases.AnyAsync(f => f.Codigo == codigo))
                    {
                        context.Fases.Add(new Fase
                        {
                            Codigo = codigo,
                            Nombre = nombre ?? string.Empty,
                            Fechas = fechas
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // 4. Seed Grupos
            if (root.TryGetProperty("grupos", out var gruposElement))
            {
                foreach (var item in gruposElement.EnumerateArray())
                {
                    var codigo = item.GetProperty("codigo").GetString();
                    var nombre = item.GetProperty("nombre").GetString();

                    if (codigo != null && !await context.Grupos.AnyAsync(g => g.Codigo == codigo))
                    {
                        context.Grupos.Add(new Grupo
                        {
                            Codigo = codigo,
                            Nombre = nombre ?? string.Empty
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // 5. Seed Sedes
            if (root.TryGetProperty("sedes", out var sedesElement))
            {
                foreach (var item in sedesElement.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var nombre = item.GetProperty("nombre").GetString();
                    var ciudad = item.GetProperty("ciudad").GetString();
                    var pais = item.GetProperty("pais").GetString();
                    var capacidadAprox = item.TryGetProperty("capacidadAprox", out var capVal) && capVal.ValueKind != JsonValueKind.Null ? capVal.GetInt32() : (int?)null;

                    if (!await context.Sedes.AnyAsync(s => s.Id == id))
                    {
                        context.Sedes.Add(new Sede
                        {
                            Id = id,
                            Nombre = nombre ?? string.Empty,
                            Ciudad = ciudad ?? string.Empty,
                            Pais = pais ?? string.Empty,
                            CapacidadAprox = capacidadAprox
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // 6. Seed Selecciones
            if (root.TryGetProperty("selecciones", out var seleccionesElement))
            {
                foreach (var item in seleccionesElement.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var codigoFifa = item.GetProperty("codigoFifa").GetString();
                    var nombre = item.GetProperty("nombre").GetString();
                    var grupo = item.GetProperty("grupo").GetString();
                    var confederacion = item.GetProperty("confederacion").GetString();
                    var esAnfitrion = item.GetProperty("esAnfitrion").GetBoolean();
                    var clasificacion = item.TryGetProperty("clasificacion", out var clasVal) ? clasVal.GetString() : null;

                    if (codigoFifa != null && grupo != null && !await context.Selecciones.AnyAsync(s => s.Id == id))
                    {
                        context.Selecciones.Add(new Seleccion
                        {
                            Id = id,
                            CodigoFifa = codigoFifa,
                            Nombre = nombre ?? string.Empty,
                            GrupoId = grupo,
                            Confederacion = confederacion ?? string.Empty,
                            EsAnfitrion = esAnfitrion,
                            Clasificacion = clasificacion
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // 7. Seed Partidos
            if (root.TryGetProperty("partidos", out var partidosElement))
            {
                foreach (var item in partidosElement.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var fase = item.GetProperty("fase").GetString();
                    var grupo = item.TryGetProperty("grupo", out var grpVal) && grpVal.ValueKind != JsonValueKind.Null ? grpVal.GetString() : null;
                    var seleccionLocalId = item.GetProperty("seleccionLocalId").GetInt32();
                    var seleccionVisitanteId = item.GetProperty("seleccionVisitanteId").GetInt32();
                    var fechaHoraUtc = item.GetProperty("fechaHoraUtc").GetDateTime();
                    var sedeId = item.GetProperty("sedeId").GetInt32();
                    var estado = item.TryGetProperty("estado", out var estVal) ? estVal.GetString() : "PROGRAMADO";
                    var golesLocal = item.TryGetProperty("golesLocal", out var glVal) && glVal.ValueKind != JsonValueKind.Null ? glVal.GetInt32() : (int?)null;
                    var golesVisitante = item.TryGetProperty("golesVisitante", out var gvVal) && gvVal.ValueKind != JsonValueKind.Null ? gvVal.GetInt32() : (int?)null;

                    if (fase != null && !await context.Partidos.AnyAsync(p => p.Id == id))
                    {
                        context.Partidos.Add(new Partido
                        {
                            Id = id,
                            FaseCodigo = fase,
                            GrupoCodigo = grupo,
                            EquipoLocalId = seleccionLocalId,
                            EquipoVisitanteId = seleccionVisitanteId,
                            FechaHoraUtc = fechaHoraUtc,
                            SedeId = sedeId,
                            Estado = estado,
                            GolesLocal = golesLocal,
                            GolesVisitante = golesVisitante
                        });
                    }
                }
                await context.SaveChangesAsync();
            }

            // 8. Seed Calendario
            if (root.TryGetProperty("partidos", out var partidosForCalElement))
            {
                foreach (var item in partidosForCalElement.EnumerateArray())
                {
                    var partidoId = item.GetProperty("id").GetInt32();
                    var fechaHoraEtStr = item.GetProperty("fechaHoraEt").GetString();

                    if (fechaHoraEtStr != null)
                    {
                        var dtOffset = DateTimeOffset.Parse(fechaHoraEtStr);
                        var localTime = dtOffset.DateTime;
                        var grupo = item.TryGetProperty("grupo", out var grpVal) && grpVal.ValueKind != JsonValueKind.Null ? grpVal.GetString() : null;
                        var fase = item.GetProperty("fase").GetString();

                        var obs = $"Fase: {fase}" + (grupo != null ? $", Grupo {grupo}" : "");

                        if (!await context.Calendarios.AnyAsync(c => c.PartidoId == partidoId))
                        {
                            context.Calendarios.Add(new Calendario
                            {
                                PartidoId = partidoId,
                                FechaHoraLocal = localTime,
                                ZonaHoraria = "ET (UTC-4)",
                                Observaciones = obs
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
