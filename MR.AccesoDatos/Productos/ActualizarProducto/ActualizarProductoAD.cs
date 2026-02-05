using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.AccesoDatos.Productos.ActualizarProducto
{
    public class ActualizarProductoAD
    {

        private readonly string _cn;
        public ActualizarProductoAD(string connectionString) => _cn = connectionString;
        private IDbConnection Db() => new SqlConnection(_cn);

        public async Task<bool> EjecutarAsync(ProductoDto p)
        {
            var sql = @"
UPDATE dbo.Productos
SET Nombre=@Nombre,
    Descripcion=@Descripcion,
    Marca=@Marca,
    PrecioCosto=@PrecioCosto,
    PrecioVenta=@PrecioVenta,
    CodigoBarras=@CodigoBarras,
    Estado=@Estado,
    ImageURL=@ImageURL,
    IdCategoria=@IdCategoria
WHERE IdProductos=@IdProductos;";

            using var db = Db();
            var rows = await db.ExecuteAsync(sql, p);
            return rows > 0;
        }

    }
}
