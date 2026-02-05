using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G9MotoRepuestos.Models
{
    [Table("Cierres")]
    public class Cierres
    {
        [Key]
        public int IdCierres { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public DateTime FechaRegistro { get; set; }

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = ""; // "Diario", "Semanal", "Mensual"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal BalanceTotal { get; set; }

        public int? IdUsuario { get; set; }
    }
}
