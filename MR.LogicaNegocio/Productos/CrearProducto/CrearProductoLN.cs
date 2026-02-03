using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.AccesoADatos.Productos;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.LogicaNegocio.Productos.CrearProducto
{
    public class CrearProductoLN
    {

        private readonly IProductosAD _ad;
        public CrearProductoLN(IProductosAD ad) => _ad = ad;

        public async Task<int> EjecutarAsync(ProductoDto p)
        {
            if (string.IsNullOrWhiteSpace(p.Nombre))
                throw new ArgumentException("Nombre es requerido.");

            if (p.PrecioCosto.HasValue && p.PrecioCosto.Value < 0)
                throw new ArgumentException("PrecioCosto no puede ser negativo.");

            if (p.PrecioVenta.HasValue && p.PrecioVenta.Value < 0)
                throw new ArgumentException("PrecioVenta no puede ser negativo.");

            if (p.StockActual.HasValue && p.StockActual.Value < 0)
                throw new ArgumentException("El stock no puede ser negativo.");


            // Por defecto activo si viene null
            p.Estado ??= true;

            return await _ad.CrearAsync(p);
        }

    }
}
