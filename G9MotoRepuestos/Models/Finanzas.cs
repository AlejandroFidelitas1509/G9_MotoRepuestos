using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G9MotoRepuestos.Models
{
    [Table("Finanzas")]
    public class Finanzas
    {
        [Key]
        public int IdFinanzas { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = ""; // "Ingreso" o "Egreso"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [StringLength(80)]
        public string Categoria { get; set; } = "";

        [StringLength(200)]
        public string? Descripcion { get; set; }

        public int? IdUsuario { get; set; }
    }
}
