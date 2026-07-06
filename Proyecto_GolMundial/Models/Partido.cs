using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_GolMundial.Models
{
    [Table("partidos")]
    public class Partido
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("fase_codigo")]
        [StringLength(20)]
        public string FaseCodigo { get; set; }

        [Column("grupo_codigo")]
        [StringLength(1)]
        public string? GrupoCodigo { get; set; }

        [Required]
        [Column("sede_id")]
        public int SedeId { get; set; }

        [Required]
        [Column("equipo_local_id")]
        public int EquipoLocalId { get; set; }

        [Required]
        [Column("equipo_visitante_id")]
        public int EquipoVisitanteId { get; set; }

        [Required]
        [Column("fecha_hora_utc")]
        public DateTime FechaHoraUtc { get; set; }

        [Column("estado")]
        [StringLength(20)]
        public string? Estado { get; set; } // PROGRAMADO, EN_JUEGO, FINALIZADO

        [Column("goles_local")]
        public int? GolesLocal { get; set; }

        [Column("goles_visitante")]
        public int? GolesVisitante { get; set; }
        
        [ForeignKey("FaseCodigo")]
        public Fase? Fase { get; set; }
        
        [ForeignKey("GrupoCodigo")]
        public Grupo? Grupo { get; set; }
        
        [ForeignKey("SedeId")]
        public Sede? Sede { get; set; }
        
        [ForeignKey("EquipoLocalId")]
        public Seleccion? EquipoLocal { get; set; }
        
        [ForeignKey("EquipoVisitanteId")]
        public Seleccion? EquipoVisitante { get; set; }
    }
}
