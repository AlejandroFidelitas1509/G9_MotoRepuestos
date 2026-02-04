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

        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            const string sql = @"
SELECT
    p.IdProducto,
    p.Nombre,
    p.Descripcion,
    p.Marca,
    p.PrecioCosto,
    p.PrecioVenta,
    p.CodigoBarras,
    p.Estado,
    p.ImageURL,
    p.IdCategoria,
    i.StockActual
FROM dbo.Productos p
LEFT JOIN dbo.Inventario i ON i.IdProducto = p.IdProducto
WHERE p.IdProducto = @Id;";

            using IDbConnection db = new SqlConnection(_cn);
            return await db.QueryFirstOrDefaultAsync<ProductoDto>(sql, new { Id = id });
        }


        public async Task<bool> ActualizarAsync(ProductoDto p)
        {
            const string sql = @"
BEGIN TRAN;

UPDATE dbo.Productos
SET
    Nombre = @Nombre,
    Descripcion = @Descripcion,
    Marca = @Marca,
    PrecioCosto = @PrecioCosto,
    PrecioVenta = @PrecioVenta,
    CodigoBarras = @CodigoBarras,
    Estado = @Estado,
    ImageURL = @ImageURL,
    IdCategoria = @IdCategoria
WHERE IdProducto = @IdProducto;

-- Asegura que exista fila en Inventario:
IF EXISTS (SELECT 1 FROM dbo.Inventario WHERE IdProducto = @IdProducto)
BEGIN
    UPDATE dbo.Inventario
    SET StockActual = @StockActual
    WHERE IdProducto = @IdProducto;
END
ELSE
BEGIN
    INSERT INTO dbo.Inventario (StockActual, StockMinimo, StockMaximo, IdProducto)
    VALUES (@StockActual, NULL, NULL, @IdProducto);
END

COMMIT;

SELECT 1;
";

            using IDbConnection db = new SqlConnection(_cn);

            var ok = await db.ExecuteScalarAsync<int>(sql, new
            {
                p.IdProducto,
                p.Nombre,
                p.Descripcion,
                p.Marca,
                p.PrecioCosto,
                p.PrecioVenta,
                p.CodigoBarras,
                Estado = p.Estado ?? true,
                p.ImageURL,
                p.IdCategoria,
                StockActual = p.StockActual ?? 0
            });

            return ok == 1;
        }


        public async Task<bool> CambiarEstadoAsync(int id, bool estado)
        {
            const string sql = @"
UPDATE dbo.Productos
SET Estado = @Estado
WHERE IdProducto = @IdProducto;";

            using IDbConnection db = new SqlConnection(_cn);

            var filas = await db.ExecuteAsync(sql, new
            {
                IdProducto = id,
                Estado = estado ? 1 : 0
            });

            return filas > 0;
        }
    }
}

