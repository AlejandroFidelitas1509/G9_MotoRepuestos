using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MR.Abstracciones.AccesoADatos.Bitacora;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.AccesoDatos.Bitacora
{
    public class BitacoraProductosAD : IBitacoraProductosAD
    {
        private readonly string _cn;
        public BitacoraProductosAD(string cn) => _cn = cn;

        private IDbConnection Db() => new SqlConnection(_cn);

        public async Task RegistrarAsync(BitacoraProductosDto b)
        {
            const string sql = @"
INSERT INTO dbo.Bitacora
(Fecha, Accion, TablaAfectada, IdUsuario, UsuarioNombre, RegistroId,
 Descripcion, AntesJson, DespuesJson)
VALUES
(GETDATE(), @Accion, @TablaAfectada, @IdUsuario, @UsuarioNombre,
 @RegistroId, @Descripcion, @AntesJson, @DespuesJson);";

            using var db = Db();
            await db.ExecuteAsync(sql, b);
        }

        // ✅ Listar normal (sin paginar) con filtros
        public async Task<IEnumerable<BitacoraProductosDto>> ListarAsync(
            int top = 200,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? accion = null,
            int? idUsuario = null)
        {
            var sql = @"
SELECT TOP (@Top)
    b.IdBitacora,
    b.Fecha,
    b.Accion,
    b.TablaAfectada,
    b.IdUsuario,
    b.UsuarioNombre,
    b.RegistroId,
    b.Descripcion,
    b.AntesJson,
    b.DespuesJson,
    p.Nombre AS NombreProducto
FROM dbo.Bitacora b
LEFT JOIN dbo.Productos p ON p.IdProducto = b.RegistroId
WHERE 1=1
";

            var dp = new DynamicParameters();
            dp.Add("@Top", top);

            if (desde.HasValue)
            {
                sql += " AND b.Fecha >= @Desde";
                dp.Add("@Desde", desde.Value);
            }

            if (hasta.HasValue)
            {
                sql += " AND b.Fecha < DATEADD(day, 1, @Hasta)";
                dp.Add("@Hasta", hasta.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(accion))
            {
                sql += " AND b.Accion = @Accion";
                dp.Add("@Accion", accion);
            }

            if (idUsuario.HasValue)
            {
                sql += " AND b.IdUsuario = @IdUsuario";
                dp.Add("@IdUsuario", idUsuario.Value);
            }

            sql += " ORDER BY b.Fecha DESC, b.IdBitacora DESC;";

            using var db = Db();
            return await db.QueryAsync<BitacoraProductosDto>(sql, dp);
        }

        // ✅ Listado paginado real
        public async Task<PagedResult<BitacoraProductosDto>> ListarPaginadoAsync(
            int page,
            int pageSize,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? accion = null,
            int? idUsuario = null)
        {
            if (page < 1) page = 1;

            // Si quieres fijo 15, puedes dejar esto:
            // pageSize = 15;

            if (pageSize < 5) pageSize = 5;
            if (pageSize > 100) pageSize = 100;

            var where = " WHERE 1=1 ";
            var dp = new DynamicParameters();

            if (desde.HasValue)
            {
                where += " AND b.Fecha >= @Desde ";
                dp.Add("@Desde", desde.Value);
            }

            if (hasta.HasValue)
            {
                where += " AND b.Fecha < DATEADD(day, 1, @Hasta) ";
                dp.Add("@Hasta", hasta.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(accion))
            {
                where += " AND b.Accion = @Accion ";
                dp.Add("@Accion", accion);
            }

            if (idUsuario.HasValue)
            {
                where += " AND b.IdUsuario = @IdUsuario ";
                dp.Add("@IdUsuario", idUsuario.Value);
            }

            dp.Add("@Offset", (page - 1) * pageSize);
            dp.Add("@PageSize", pageSize);

            var sqlCount = @"
SELECT COUNT(1)
FROM dbo.Bitacora b
" + where + ";";

            var sqlPage = @"
SELECT
    b.IdBitacora,
    b.Fecha,
    b.Accion,
    b.TablaAfectada,
    b.IdUsuario,
    b.UsuarioNombre,
    b.RegistroId,
    b.Descripcion,
    b.AntesJson,
    b.DespuesJson,
    p.Nombre AS NombreProducto
FROM dbo.Bitacora b
LEFT JOIN dbo.Productos p ON p.IdProducto = b.RegistroId
" + where + @"
ORDER BY b.Fecha DESC, b.IdBitacora DESC
OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using var db = Db();
            using var multi = await db.QueryMultipleAsync(sqlCount + "\n" + sqlPage, dp);

            var total = await multi.ReadFirstAsync<int>();
            var items = (await multi.ReadAsync<BitacoraProductosDto>()).ToList();

            return new PagedResult<BitacoraProductosDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<string>> ListarAccionesAsync()
        {
            const string sql = @"
SELECT DISTINCT Accion
FROM dbo.Bitacora
WHERE Accion IS NOT NULL
ORDER BY Accion;";

            using var db = Db();
            return await db.QueryAsync<string>(sql);
        }

        public async Task<IEnumerable<(int IdUsuario, string Nombre)>> ListarUsuariosAsync()
        {
            const string sql = @"
SELECT DISTINCT
    IdUsuario AS IdUsuario,
    ISNULL(UsuarioNombre, 'N/A') AS Nombre
FROM dbo.Bitacora
WHERE IdUsuario IS NOT NULL
ORDER BY Nombre;";

            using var db = Db();
            return await db.QueryAsync<(int IdUsuario, string Nombre)>(sql);
        }
    }
}