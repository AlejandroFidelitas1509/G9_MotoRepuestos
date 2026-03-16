using Dapper;
using Microsoft.Data.SqlClient;
using MR.Abstracciones.AccesoADatos.Finanzas;
using MR.Abstracciones.EntidadesParaUI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.AccesoDatos.Finanzas
{
    public class FinanzasAD : IFinanzasAD
    {

        private readonly string _cadenaConexion;

        public FinanzasAD(string cadenaConexion)
        {
            _cadenaConexion = cadenaConexion;
        }

        private IDbConnection Db()
        {
            return new SqlConnection(_cadenaConexion);
        }

        public async Task<IEnumerable<MovimientoFinancieroDto>> ObtenerMovimientosAsync(
            string? textoBusqueda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoFiltro,
            string? origenFiltro)
        {
            const string sql = @"
SELECT
    f.IdFinanzas,
    f.Tipo,
    f.Monto,
    f.Fecha,
    f.Categoria,
    f.Descripcion,
    f.IdUsuario,
    f.Origen,
    f.IdReferencia,
    u.NombreCompleto AS UsuarioNombre
FROM dbo.Finanzas f
LEFT JOIN dbo.Usuarios u ON u.IdUsuario = f.IdUsuario
WHERE
    (@TextoBusqueda IS NULL OR
        f.Categoria LIKE '%' + @TextoBusqueda + '%' OR
        f.Descripcion LIKE '%' + @TextoBusqueda + '%' OR
        f.Tipo LIKE '%' + @TextoBusqueda + '%' OR
        f.Origen LIKE '%' + @TextoBusqueda + '%')
    AND (@Desde IS NULL OR CAST(f.Fecha AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(f.Fecha AS DATE) <= CAST(@Hasta AS DATE))
    AND (@TipoFiltro IS NULL OR f.Tipo = @TipoFiltro)
    AND (@OrigenFiltro IS NULL OR f.Origen = @OrigenFiltro)
ORDER BY f.Fecha DESC, f.IdFinanzas DESC;";

            using var db = Db();
            return await db.QueryAsync<MovimientoFinancieroDto>(sql, new
            {
                TextoBusqueda = string.IsNullOrWhiteSpace(textoBusqueda) ? null : textoBusqueda.Trim(),
                Desde = desde,
                Hasta = hasta,
                TipoFiltro = string.IsNullOrWhiteSpace(tipoFiltro) ? null : tipoFiltro.Trim(),
                OrigenFiltro = string.IsNullOrWhiteSpace(origenFiltro) ? null : origenFiltro.Trim()
            });
        }

        public async Task<ResumenContabilidadDto> ObtenerResumenAsync(
            DateTime? desde,
            DateTime? hasta,
            string? tipoFiltro,
            string? origenFiltro)
        {
            const string sql = @"
SELECT
    ISNULL(SUM(CASE WHEN f.Tipo = 'Ingreso' THEN f.Monto ELSE 0 END), 0) AS TotalIngresos,
    ISNULL(SUM(CASE WHEN f.Tipo = 'Egreso' THEN f.Monto ELSE 0 END), 0) AS TotalEgresos
FROM dbo.Finanzas f
WHERE
    (@Desde IS NULL OR CAST(f.Fecha AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(f.Fecha AS DATE) <= CAST(@Hasta AS DATE))
    AND (@TipoFiltro IS NULL OR f.Tipo = @TipoFiltro)
    AND (@OrigenFiltro IS NULL OR f.Origen = @OrigenFiltro);";

            using var db = Db();
            var resultado = await db.QueryFirstOrDefaultAsync<ResumenContabilidadDto>(sql, new
            {
                Desde = desde,
                Hasta = hasta,
                TipoFiltro = string.IsNullOrWhiteSpace(tipoFiltro) ? null : tipoFiltro.Trim(),
                OrigenFiltro = string.IsNullOrWhiteSpace(origenFiltro) ? null : origenFiltro.Trim()
            });

            return resultado ?? new ResumenContabilidadDto();
        }

        public async Task<int> RegistrarMovimientoAsync(MovimientoFinancieroDto movimiento)
        {
            const string sql = @"
INSERT INTO dbo.Finanzas
(
    Tipo,
    Monto,
    Fecha,
    Categoria,
    Descripcion,
    IdUsuario,
    Origen,
    IdReferencia
)
VALUES
(
    @Tipo,
    @Monto,
    @Fecha,
    @Categoria,
    @Descripcion,
    @IdUsuario,
    @Origen,
    @IdReferencia
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var db = Db();
            return await db.ExecuteScalarAsync<int>(sql, movimiento);
        }

        public async Task<IEnumerable<CierreContableDto>> ObtenerCierresAsync()
        {
            const string sql = @"
SELECT
    c.IdCierres AS IdCierre,
    c.FechaInicio,
    c.FechaFin,
    c.FechaRegistro,
    c.Tipo,
    c.TotalIngresos,
    c.TotalEgresos,
    c.BalanceTotal,
    c.IdUsuario,
    u.NombreCompleto AS UsuarioNombre
FROM dbo.Cierres c
LEFT JOIN dbo.Usuarios u ON u.IdUsuario = c.IdUsuario
ORDER BY c.FechaRegistro DESC, c.IdCierres DESC;";

            using var db = Db();
            return await db.QueryAsync<CierreContableDto>(sql);
        }

        public async Task<int> RegistrarCierreAsync(CierreContableDto cierre)
        {
            const string sql = @"
INSERT INTO dbo.Cierres
(
    FechaInicio,
    FechaFin,
    FechaRegistro,
    Tipo,
    TotalIngresos,
    TotalEgresos,
    BalanceTotal,
    IdUsuario
)
VALUES
(
    @FechaInicio,
    @FechaFin,
    @FechaRegistro,
    @Tipo,
    @TotalIngresos,
    @TotalEgresos,
    @BalanceTotal,
    @IdUsuario
);

SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var db = Db();
            return await db.ExecuteScalarAsync<int>(sql, cierre);
        }

    }
}
