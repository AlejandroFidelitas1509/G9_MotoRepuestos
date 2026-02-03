using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.AccesoADatos.Productos;
using MR.Abstracciones.EntidadesParaUI;
using MR.AccesoDatos.Productos.CrearProducto;

namespace MR.AccesoDatos.Productos
{
    public class ProductosAD : IProductosAD
    {
        private readonly string _cn;

        public ProductosAD(string cn)
        {
            _cn = cn;
        }

        // Implementado
        public Task<int> CrearAsync(ProductoDto producto)
            => new CrearProductoAD(_cn).EjecutarAsync(producto);

        //  Pendientes (por ahora)
        public Task<IEnumerable<ProductoDto>> ListarAsync(bool soloActivos = true)
            => throw new NotImplementedException("ListarAsync aún no implementado.");

        public Task<ProductoDto?> ObtenerPorIdAsync(int id)
            => throw new NotImplementedException("ObtenerPorIdAsync aún no implementado.");

        public Task<bool> ActualizarAsync(ProductoDto producto)
            => throw new NotImplementedException("ActualizarAsync aún no implementado.");

        public Task<bool> CambiarEstadoAsync(int id, bool estado)
            => throw new NotImplementedException("CambiarEstadoAsync aún no implementado.");
    }
}

