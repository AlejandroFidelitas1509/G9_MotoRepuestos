using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class ProductoDto
    {
        public int IdProductos { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Marca { get; set; }
        public decimal? PrecioCosto { get; set; }
        public decimal? PrecioVenta { get; set; }
        public string? CodigoBarras { get; set; }
        public bool? Estado { get; set; }
        public string? ImageURL { get; set; }
        public int? IdCategoria { get; set; }
    }
}
