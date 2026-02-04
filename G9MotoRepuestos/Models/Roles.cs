using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G9MotoRepuestos.Models
{
    [Table("Roles", Schema = "dbo")]
    public class Rol
    {
        [Key]
        public int IdRol { get; set; }
        public string? Tipo { get; set; }
    }
}
