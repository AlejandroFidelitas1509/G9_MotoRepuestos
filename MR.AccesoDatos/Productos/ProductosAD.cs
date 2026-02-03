using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
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

        public async Task<IEnumerable<ProductoDto>> ListarAsync(bool soloActivos = true)
        {
            const string sql = @"
SELECT
    p.IdProducto,
    p.Nombre,
    p.Marca,
    p.PrecioVenta,
    p.Estado,
    i.StockActual
FROM dbo.Productos p
LEFT JOIN dbo.Inventario i ON i.IdProducto = p.IdProducto
WHERE (@soloActivos = 0 OR ISNULL(p.Estado, 0) = 1)
ORDER BY p.IdProducto DESC;";

            using IDbConnection db = new SqlConnection(_cn);

            return await db.QueryAsync<ProductoDto>(sql, new
            {
                soloActivos = soloActivos ? 1 : 0
            });
        }

        //  Pendientes (por ahora)

        public Task<ProductoDto?> ObtenerPorIdAsync(int id)
            => throw new NotImplementedException("ObtenerPorIdAsync aún no implementado.");

        public Task<bool> ActualizarAsync(ProductoDto producto)
            => throw new NotImplementedException("ActualizarAsync aún no implementado.");

        public Task<bool> CambiarEstadoAsync(int id, bool estado)
            => throw new NotImplementedException("CambiarEstadoAsync aún no implementado.");
    }
}

