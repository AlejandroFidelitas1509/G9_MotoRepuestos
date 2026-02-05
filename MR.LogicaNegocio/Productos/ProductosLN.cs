using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.AccesoADatos.Productos;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Productos;
using MR.LogicaNegocio.Productos.CrearProducto;

namespace MR.LogicaNegocio.Productos
{
    public class ProductosLN : IProductosLN
    {

        private readonly IProductosAD _ad;

        public ProductosLN(IProductosAD ad)
        {
            _ad = ad;
        }

        //  Implementado
        public Task<int> CrearAsync(ProductoDto producto)
            => new CrearProductoLN(_ad).EjecutarAsync(producto);

        public Task<IEnumerable<ProductoDto>> ListarAsync(bool soloActivos = true)
            => _ad.ListarAsync(soloActivos);

        public Task<ProductoDto?> ObtenerPorIdAsync(int id)
            => _ad.ObtenerPorIdAsync(id);

        public async Task<bool> ActualizarAsync(ProductoDto p)
        {
            if (p.IdProducto <= 0) throw new ArgumentException("Id inválido.");
            if (string.IsNullOrWhiteSpace(p.Nombre)) throw new ArgumentException("Nombre es requerido.");
            if (string.IsNullOrWhiteSpace(p.Marca)) throw new ArgumentException("Marca es requerida.");
            if (!p.PrecioVenta.HasValue || p.PrecioVenta.Value <= 0) throw new ArgumentException("PrecioVenta debe ser mayor a 0.");
            if (!p.StockActual.HasValue || p.StockActual.Value < 0) throw new ArgumentException("Stock inválido.");

            return await _ad.ActualizarAsync(p);
        }
        
        public Task<bool> DesactivarAsync(int id)
            => _ad.CambiarEstadoAsync(id, false);

        public Task<bool> ActivarAsync(int id)
            => _ad.CambiarEstadoAsync(id, true);
    }

}

