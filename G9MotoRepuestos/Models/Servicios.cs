using System.ComponentModel.DataAnnotations;

namespace G9MotoRepuestos.Models
{
    public class Servicios
    {
        [Key]
        public int IdServicio { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string ImagenUrl { get; set; }
        public bool Estado { get; set; }
    }
}
