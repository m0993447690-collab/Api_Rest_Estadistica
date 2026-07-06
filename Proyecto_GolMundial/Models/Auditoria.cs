using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_GolMundial.Models
{
    [Table("auditoria")]
    public class Auditoria
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("accion")]
        [StringLength(100)]
        public string Accion { get; set; }

        [Required]
        [Column("entidad")]
        [StringLength(50)]
        public string Entidad { get; set; }

        [Column("entidad_id")]
        public int? EntidadId { get; set; }

        [Required]
        [Column("fecha_hora_utc")]
        public DateTime FechaHoraUtc { get; set; }

        [Column("detalles")]
        public string? Detalles { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }
    }
}
