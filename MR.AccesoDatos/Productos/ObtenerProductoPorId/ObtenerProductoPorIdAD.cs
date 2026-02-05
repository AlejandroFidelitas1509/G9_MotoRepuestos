using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.AccesoDatos.Productos.ObtenerProductoPorId
{
    public class ObtenerProductoPorIdAD
    {

        private readonly string _cn;
        public ObtenerProductoPorIdAD(string connectionString) => _cn = connectionString;
        private IDbConnection Db() => new SqlConnection(_cn);

        public async Task<ProductoDto?> EjecutarAsync(int id)
        {
            var sql = @"
SELECT TOP 1 IdProductos, Nombre, Descripcion, Marca, PrecioCosto, PrecioVenta, CodigoBarras, Estado, ImageURL, IdCategoria
FROM dbo.Productos
WHERE IdProductos = @id;";

            using var db = Db();
            return await db.QueryFirstOrDefaultAsync<ProductoDto>(sql, new { id });
        }

    }
}
