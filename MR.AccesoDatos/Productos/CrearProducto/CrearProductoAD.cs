using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.AccesoDatos.Productos.CrearProducto
{
    public class CrearProductoAD
    {

        private readonly string _cn;
        public CrearProductoAD(string cn) => _cn = cn;

        private IDbConnection Db() => new SqlConnection(_cn);

        public async Task<int> EjecutarAsync(ProductoDto p)
        {
            const string sql = @"
INSERT INTO dbo.Productos
(Nombre, Descripcion, Marca, PrecioCosto, PrecioVenta, CodigoBarras, Estado, ImageURL, IdCategoria)
VALUES
(@Nombre, @Descripcion, @Marca, @PrecioCosto, @PrecioVenta, @CodigoBarras, @Estado, @ImageURL, @IdCategoria);

SELECT CAST(SCOPE_IDENTITY() as int);";

            using var db = Db();

            return await db.ExecuteScalarAsync<int>(sql, new
            {
                p.Nombre,
                p.Descripcion,
                p.Marca,
                p.PrecioCosto,
                p.PrecioVenta,
                p.CodigoBarras,
                Estado = p.Estado ?? true,
                p.ImageURL,
                p.IdCategoria
            });
        }
    }
}
