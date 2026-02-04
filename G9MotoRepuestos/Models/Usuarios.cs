using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace G9MotoRepuestos.Models
{
    [Table("Usuarios", Schema = "dbo")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? PasswordHash { get; set; }
        public bool? Estado { get; set; }
        public string? ImagenURL { get; set; }
        public int? IdRol { get; set; }

        [ForeignKey("IdRol")]
        public virtual Rol? Rol { get; set; }
    }
}
