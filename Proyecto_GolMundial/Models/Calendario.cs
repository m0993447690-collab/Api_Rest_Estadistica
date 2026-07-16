using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_GolMundial.Models
{
    [Table("calendario")]
    public class Calendario
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("partido_id")]
        public int PartidoId { get; set; }

        [Required]
        [Column("fecha_hora_local")]
        public DateTime FechaHoraLocal { get; set; }

        [Required]
        [Column("zona_horaria")]
        [StringLength(50)]
        public string ZonaHoraria { get; set; }

        [Column("observaciones")]
        [StringLength(255)]
        public string? Observaciones { get; set; }

        [ForeignKey("PartidoId")]
        public Partido? Partido { get; set; }
    }
}
