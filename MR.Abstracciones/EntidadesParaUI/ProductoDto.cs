using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class ProductoDto
    {
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string? Nombre { get; set; }

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        public string? Marca { get; set; }

        public decimal? PrecioCosto { get; set; }

        [Required(ErrorMessage = "El precio de venta es obligatorio")]
        [Range(0.01, 999999999, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal? PrecioVenta { get; set; }

        public string? CodigoBarras { get; set; }
        public bool? Estado { get; set; }
        public string? ImageURL { get; set; }
        public int? IdCategoria { get; set; }

        [Required(ErrorMessage = "El stock inicial es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int? StockActual { get; set; }

    }
}
