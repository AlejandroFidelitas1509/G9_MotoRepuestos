using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.AccesoDatos.Productos.ListarProductos
{
    public class ListarProductosAD
    {

        private readonly string _cn;
        public ListarProductosAD(string connectionString) => _cn = connectionString;
        private IDbConnection Db() => new SqlConnection(_cn);

        public async Task<IEnumerable<ProductoDto>> EjecutarAsync(bool soloActivos)
        {
            var sql = @"
SELECT IdProductos, Nombre, Descripcion, Marca, PrecioCosto, PrecioVenta, CodigoBarras, Estado, ImageURL, IdCategoria
FROM dbo.Productos
WHERE (@soloActivos = 0) OR (ISNULL(Estado,0)=1)
ORDER BY IdProductos DESC;";

            using var db = Db();
            return await db.QueryAsync<ProductoDto>(sql, new { soloActivos = soloActivos ? 1 : 0 });
        }

    }
}
