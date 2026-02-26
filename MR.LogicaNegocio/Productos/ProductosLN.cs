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

        public Task<int> CrearAsync(ProductoDto producto)
            => _ad.CrearAsync(producto);

        public Task<IEnumerable<ProductoDto>> ListarAsync(bool soloActivos = true)
            => _ad.ListarAsync(soloActivos);

        public Task<ProductoDto?> ObtenerPorIdAsync(int id)
            => _ad.ObtenerPorIdAsync(id);

        public Task<bool> ActualizarAsync(ProductoDto producto)
            => _ad.ActualizarAsync(producto);

        public Task<bool> CambiarEstadoAsync(int id, bool estado)
            => _ad.CambiarEstadoAsync(id, estado);
    }

}

