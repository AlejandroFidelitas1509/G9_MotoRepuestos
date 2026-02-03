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

        //  Pendientes por ahora
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

