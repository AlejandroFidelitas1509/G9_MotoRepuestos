using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public async Task RegistrarAsync(string accion, string tablaAfectada, int? idUsuario = null)
        {
            const string sql = @"
INSERT INTO dbo.Bitacora (Fecha, Accion, TablaAfectada, IdUsuario)
VALUES (GETDATE(), @Accion, @TablaAfectada, @IdUsuario);";

            using var db = Db();
            await db.ExecuteAsync(sql, new
            {
                Accion = accion,
                TablaAfectada = tablaAfectada,
                IdUsuario = idUsuario
            });
        }

        public async Task<IEnumerable<BitacoraProductosDto>> ListarAsync(int top = 100)
        {
            const string sql = @"
SELECT TOP (@Top)
    IdBitacora,
    Fecha,
    Accion,
    TablaAfectada,
    IdUsuario
FROM dbo.Bitacora
ORDER BY Fecha DESC, IdBitacora DESC;";

            using var db = Db();
            return await db.QueryAsync<BitacoraProductosDto>(sql, new { Top = top });
        }
    }

}

