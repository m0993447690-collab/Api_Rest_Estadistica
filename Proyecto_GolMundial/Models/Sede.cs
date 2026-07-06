using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_GolMundial.Models
{
    [Table("sedes")]
    public class Sede
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(150)]
        public string Nombre { get; set; }

        [Required]
        [Column("ciudad")]
        [StringLength(100)]
        public string Ciudad { get; set; }

        [Required]
        [Column("pais")]
        [StringLength(100)]
        public string Pais { get; set; }

        [Column("capacidad_aprox")]
        public int? CapacidadAprox { get; set; }
    }
}
