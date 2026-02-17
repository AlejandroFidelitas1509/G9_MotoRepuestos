using Dapper;
using Microsoft.Data.SqlClient;
using MR.Abstracciones.AccesoADatos.Categorias;
using MR.Abstracciones.EntidadesParaUI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.AccesoDatos.Categorias
{
    public class CategoriasAD : ICategoriasAD
    {
        private readonly string _cn;
        public CategoriasAD(string cn) => _cn = cn;
        private IDbConnection Db() => new SqlConnection(_cn);

        public async Task<IEnumerable<CategoriaDto>> ListarAsync(bool soloActivas = true)
        {
            const string sql = @"
SELECT IdCategoria, Nombre, Estado
FROM dbo.Categorias
WHERE (@soloActivas = 0) OR (ISNULL(Estado,1)=1)
ORDER BY Nombre;";

            using var db = Db();
            return await db.QueryAsync<CategoriaDto>(sql, new { soloActivas = soloActivas ? 1 : 0 });
        }

        public async Task<int> CrearAsync(CategoriaDto categoria)
        {
            const string sql = @"
INSERT INTO dbo.Categorias (Nombre, Estado)
VALUES (@Nombre, 1);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var db = Db();
            return await db.ExecuteScalarAsync<int>(sql, new { categoria.Nombre });
        }

        public async Task<bool> CambiarEstadoAsync(int idCategoria, bool estado)
        {
            const string sql = @"
UPDATE dbo.Categorias
SET Estado = @Estado
WHERE IdCategoria = @IdCategoria;";

            using var db = Db();
            var filas = await db.ExecuteAsync(sql, new { IdCategoria = idCategoria, Estado = estado ? 1 : 0 });
            return filas > 0;
        }
    }
}
