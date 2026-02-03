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

        //  Pendientes por ahora
        public Task<IEnumerable<ProductoDto>> ListarAsync(bool soloActivos = true)
            => throw new NotImplementedException("ListarAsync aún no implementado.");

        public Task<ProductoDto?> ObtenerPorIdAsync(int id)
            => throw new NotImplementedException("ObtenerPorIdAsync aún no implementado.");

        public Task<bool> ActualizarAsync(ProductoDto producto)
            => throw new NotImplementedException("ActualizarAsync aún no implementado.");

        public Task<bool> ActivarAsync(int id)
            => throw new NotImplementedException("ActivarAsync aún no implementado.");

        public Task<bool> DesactivarAsync(int id)
            => throw new NotImplementedException("DesactivarAsync aún no implementado.");
    }

}

