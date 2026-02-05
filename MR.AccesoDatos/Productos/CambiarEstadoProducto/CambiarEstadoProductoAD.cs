using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace MR.AccesoDatos.Productos.CambiarEstadoProducto
{
    public class CambiarEstadoProductoAD
    {

        private readonly string _cn;
        public CambiarEstadoProductoAD(string connectionString) => _cn = connectionString;
        private IDbConnection Db() => new SqlConnection(_cn);

        public async Task<bool> EjecutarAsync(int id, bool estado)
        {
            var sql = @"UPDATE dbo.Productos SET Estado=@estado WHERE IdProductos=@id;";
            using var db = Db();
            var rows = await db.ExecuteAsync(sql, new { id, estado });
            return rows > 0;
        }

    }
}
