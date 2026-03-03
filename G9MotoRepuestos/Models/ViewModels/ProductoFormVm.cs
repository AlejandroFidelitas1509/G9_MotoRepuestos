using System.ComponentModel.DataAnnotations;

namespace G9MotoRepuestos.Models.ViewModels
{
    public class ProductoFormVm
    {
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(120, ErrorMessage = "Máximo 120 caracteres")]
        public string? Nombre { get; set; }

        [StringLength(250, ErrorMessage = "Máximo 250 caracteres")]
        public string? Descripcion { get; set; }

        [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
        public string? Marca { get; set; }

        [StringLength(60, ErrorMessage = "Máximo 60 caracteres")]
        public string? CodigoBarras { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal PrecioVenta { get; set; }

        // Si es servicio, no requiere stock
        public bool EsServicio { get; set; }

        [Range(0, 999999, ErrorMessage = "Stock inválido")]
        public int StockActual { get; set; }
    }
}

