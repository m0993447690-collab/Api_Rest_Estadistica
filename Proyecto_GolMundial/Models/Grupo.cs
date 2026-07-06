using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto_GolMundial.Models
{
    [Table("grupos")]
    public class Grupo
    {
        [Key]
        [Column("codigo")]
        [StringLength(1)]
        public string Codigo { get; set; }

        [Required]
        [Column("nombre")]
        [StringLength(50)]
        public string Nombre { get; set; }
    }
}
