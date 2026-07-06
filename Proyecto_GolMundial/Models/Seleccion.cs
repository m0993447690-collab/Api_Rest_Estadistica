using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_GolMundial.Models
{
    [Table("selecciones")]
    public class Seleccion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("codigo_fifa")]
        [StringLength(3)]
        public string CodigoFifa { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [Column("grupo")]
        [StringLength(1)]
        public string GrupoId { get; set; }

        [Required]
        [Column("confederacion")]
        [StringLength(50)]
        public string Confederacion { get; set; }

        [Column("es_anfitrion")]
        public bool EsAnfitrion { get; set; }

        [Column("clasificacion")]
        [StringLength(150)]
        public string? Clasificacion { get; set; }
        
        [ForeignKey("GrupoId")]
        public Grupo? Grupo { get; set; }
    }
}
