using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_GolMundial.Models
{
    [Table("fases")]
    public class Fase
    {
        [Key]
        [Column("codigo")]
        [StringLength(20)]
        public string Codigo { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Column("fechas")]
        [StringLength(100)]
        public string? Fechas { get; set; }
    }
}
