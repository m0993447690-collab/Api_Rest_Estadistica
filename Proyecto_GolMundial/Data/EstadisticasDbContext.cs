using Microsoft.EntityFrameworkCore;
using Proyecto_GolMundial.Models;

namespace Proyecto_GolMundial.Data
{
    public class EstadisticasDbContext : DbContext
    {
        public EstadisticasDbContext(DbContextOptions<EstadisticasDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Seleccion> Selecciones { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Sede> Sedes { get; set; }
        public DbSet<Fase> Fases { get; set; }
        public DbSet<Partido> Partidos { get; set; }
        public DbSet<Auditoria> AuditoriaRegistros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Aquí se pueden agregar restricciones adicionales usando Fluent API si es necesario
        }
    }
}
