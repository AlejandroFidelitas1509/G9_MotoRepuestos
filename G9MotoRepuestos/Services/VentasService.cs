using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using G9MotoRepuestos.Models.ViewModels;

namespace G9MotoRepuestos.Services
{
    public interface IVentasService
    {
        Task<CartItemVm?> BuscarProductoAsync(string query);
        Task<(bool ok, string? error)> ValidarStockAsync(int idProducto, int cantidad);
        Task<int> CrearVentaAsync(int? idUsuario, string formaPago, decimal subtotal, decimal impuesto, decimal descuento, decimal total, IEnumerable<CartItemVm> items);
        Task<FacturaVm?> ObtenerFacturaAsync(int idVenta);
        Task<(bool ok, string? error)> AnularVentaAsync(int idVenta, int? idUsuario, string motivo);
        Task<List<AuditoriaVentasVm>> AuditoriaAsync(DateTime? desde, DateTime? hasta, string? accion);
        Task<Dictionary<DateTime, decimal>> TotalesDiariosAsync(DateTime desde, DateTime hasta);
    }

    public class VentasService : IVentasService
    {
        private readonly string _cn;
        public VentasService(IConfiguration config) => _cn = config.GetConnectionString("DefaultConnection")!;

        private IDbConnection Conn() => new SqlConnection(_cn);

        // ✅ Buscar por nombre o código (stock real desde Inventario)
        public async Task<CartItemVm?> BuscarProductoAsync(string query)
        {

            const string sql = @"
SELECT TOP 1
    p.IdProducto      AS Id,
    p.CodigoBarras    AS Codigo,
    p.Nombre          AS Nombre,
    p.PrecioVenta     AS Precio,
    ISNULL(i.StockActual, 0) AS Stock
FROM dbo.Productos p
LEFT JOIN dbo.Inventario i ON i.IdProducto = p.IdProducto
WHERE (p.CodigoBarras = @q OR p.Nombre LIKE '%' + @q + '%')
  AND ISNULL(p.Estado,0) = 1
ORDER BY p.IdProducto DESC;";

            using var db = Conn();
            var item = await db.QueryFirstOrDefaultAsync<CartItemVm>(sql, new { q = query.Trim() });
            if (item == null) return null;

            item.Cantidad = 1;
            return item;
        }

        // ✅ Stock real (Inventario)
        public async Task<(bool ok, string? error)> ValidarStockAsync(int idProducto, int cantidad)
        {
            const string sql = @"SELECT ISNULL(StockActual,0) FROM dbo.Inventario WHERE IdProducto=@id;";
            using var db = Conn();

            var stock = await db.ExecuteScalarAsync<int?>(sql, new { id = idProducto });
            var s = stock ?? 0;

            if (s <= 0) return (false, "Producto no disponible en inventario");
            if (cantidad > s) return (false, $"Stock insuficiente. Máximo disponible: {s}");
            return (true, null);
        }

        // ✅ Crear venta + detalle + rebaja inventario + finanzas + auditoría
        public async Task<int> CrearVentaAsync(int? idUsuario, string formaPago, decimal subtotal, decimal impuesto, decimal descuento, decimal total, IEnumerable<CartItemVm> items)
        {
            using var db = Conn();
            using var tx = db.BeginTransaction();

            try
            {
                var idVenta = await db.ExecuteScalarAsync<int>(@"
INSERT INTO dbo.Ventas (Fecha, IdUsuario, FormaPago, Subtotal, Impuesto, Descuento, Total, Estado)
VALUES (SYSUTCDATETIME(), @IdUsuario, @FormaPago, @Subtotal, @Impuesto, @Descuento, @Total, 'Activa');
SELECT CAST(SCOPE_IDENTITY() AS int);",
                    new { IdUsuario = idUsuario, FormaPago = formaPago, Subtotal = subtotal, Impuesto = impuesto, Descuento = descuento, Total = total }, tx);

                foreach (var it in items)
                {
                    var stockOk = await ValidarStockAsync(it.Id, it.Cantidad);
                    if (!stockOk.ok) throw new InvalidOperationException(stockOk.error);

                    await db.ExecuteAsync(@"
INSERT INTO dbo.VentaDetalle (IdVenta, IdProducto, Codigo, NombreProducto, Cantidad, PrecioUnitario, SubtotalLinea)
VALUES (@IdVenta, @IdProducto, @Codigo, @Nombre, @Cantidad, @Precio, @SubtotalLinea);",
                        new
                        {
                            IdVenta = idVenta,
                            IdProducto = it.Id,
                            Codigo = it.Codigo,
                            Nombre = it.Nombre,
                            Cantidad = it.Cantidad,
                            Precio = it.Precio,
                            SubtotalLinea = it.Precio * it.Cantidad
                        }, tx);

                    // ✅ Rebaja segura en Inventario (evita carreras)
                    var filas = await db.ExecuteAsync(@"
UPDATE dbo.Inventario
SET StockActual = StockActual - @Cant
WHERE IdProducto = @IdProducto
  AND StockActual >= @Cant;",
                        new { Cant = it.Cantidad, IdProducto = it.Id }, tx);

                    if (filas == 0)
                        throw new InvalidOperationException("Stock insuficiente al confirmar la venta (inventario cambió).");
                }

                // ✅ Finanzas: ingreso para cierres contables
                await db.ExecuteAsync(@"
INSERT INTO dbo.Finanzas (Tipo, Monto, Fecha, Categoria, Descripcion)
VALUES ('Ingreso', @Monto, CAST(GETDATE() AS date), 'Ventas', CONCAT('Venta #', @IdVenta));",
                    new { Monto = total, IdVenta = idVenta }, tx);

                // ✅ Auditoría
                await db.ExecuteAsync(@"
INSERT INTO dbo.AuditoriaVentas (Accion, IdVenta, IdUsuario, Descripcion)
VALUES ('CREAR', @IdVenta, @IdUsuario, CONCAT('Venta creada. Total: ', @Total));",
                    new { IdVenta = idVenta, IdUsuario = idUsuario, Total = total }, tx);

                tx.Commit();
                return idVenta;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ✅ Obtener factura + detalle
        public async Task<FacturaVm?> ObtenerFacturaAsync(int idVenta)
        {
            using var db = Conn();

            var venta = await db.QueryFirstOrDefaultAsync<FacturaVm>(@"
SELECT IdVenta, Fecha, FormaPago, Subtotal, Impuesto, Descuento, Total, Estado
FROM dbo.Ventas
WHERE IdVenta = @id;", new { id = idVenta });

            if (venta == null) return null;

            var detalle = (await db.QueryAsync<FacturaLineaVm>(@"
SELECT ISNULL(Codigo,'') AS Codigo, NombreProducto, Cantidad, PrecioUnitario, SubtotalLinea
FROM dbo.VentaDetalle
WHERE IdVenta = @id
ORDER BY IdDetalle;", new { id = idVenta })).ToList();

            venta.Detalle = detalle;
            return venta;
        }

        // ✅ Anular venta: repone inventario + reversa finanzas + auditoría
        public async Task<(bool ok, string? error)> AnularVentaAsync(int idVenta, int? idUsuario, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Datos incompletos para hacer la devolución");

            using var db = Conn();
            using var tx = db.BeginTransaction();

            try
            {
                var venta = await db.QueryFirstOrDefaultAsync<(string Estado, decimal Total)>(
                    @"SELECT Estado, Total FROM dbo.Ventas WHERE IdVenta=@id;",
                    new { id = idVenta }, tx);

                if (string.IsNullOrEmpty(venta.Estado))
                    return (false, "La factura no existe");

                if (venta.Estado == "Anulada")
                    return (false, "La factura ya fue anulada");

                var detalle = (await db.QueryAsync<(int IdProducto, int Cantidad)>(@"
SELECT ISNULL(IdProducto,0) AS IdProducto, Cantidad
FROM dbo.VentaDetalle
WHERE IdVenta=@id;", new { id = idVenta }, tx)).ToList();

                foreach (var d in detalle.Where(x => x.IdProducto != 0))
                {
                    // ✅ Reponer stock en Inventario
                    await db.ExecuteAsync(@"
UPDATE dbo.Inventario
SET StockActual = StockActual + @Cant
WHERE IdProducto = @IdProducto;",
                        new { Cant = d.Cantidad, IdProducto = d.IdProducto }, tx);
                }

                await db.ExecuteAsync(@"UPDATE dbo.Ventas SET Estado='Anulada' WHERE IdVenta=@id;", new { id = idVenta }, tx);

                await db.ExecuteAsync(@"INSERT INTO dbo.AnulacionesVenta (IdVenta, Motivo) VALUES (@IdVenta, @Motivo);",
                    new { IdVenta = idVenta, Motivo = motivo }, tx);

                // ✅ Finanzas: reverso
                await db.ExecuteAsync(@"
INSERT INTO dbo.Finanzas (Tipo, Monto, Fecha, Categoria, Descripcion)
VALUES ('Egreso', @Monto, CAST(GETDATE() AS date), 'Anulaciones', CONCAT('Anulación venta #', @IdVenta));",
                    new { Monto = venta.Total, IdVenta = idVenta }, tx);

                // ✅ Auditoría
                await db.ExecuteAsync(@"
INSERT INTO dbo.AuditoriaVentas (Accion, IdVenta, IdUsuario, Descripcion)
VALUES ('ANULAR', @IdVenta, @IdUsuario, @Desc);",
                    new { IdVenta = idVenta, IdUsuario = idUsuario, Desc = "Factura anulada. Motivo: " + motivo }, tx);

                tx.Commit();
                return (true, null);
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return (false, ex.Message);
            }
        }

        // ✅ Auditoría con filtros
        public async Task<List<AuditoriaVentasVm>> AuditoriaAsync(DateTime? desde, DateTime? hasta, string? accion)
        {
            using var db = Conn();

            var sql = @"
SELECT IdAuditoria, Accion, IdVenta, Fecha, IdUsuario, Descripcion
FROM dbo.AuditoriaVentas
WHERE 1=1
  AND (@accion IS NULL OR Accion = @accion)
  AND (@desde IS NULL OR Fecha >= @desde)
  AND (@hasta IS NULL OR Fecha < DATEADD(day,1,@hasta))
ORDER BY Fecha DESC;";

            return (await db.QueryAsync<AuditoriaVentasVm>(sql, new
            {
                accion = string.IsNullOrWhiteSpace(accion) ? null : accion,
                desde,
                hasta
            })).ToList();
        }

        // ✅ Totales diarios (ventas activas)
        public async Task<Dictionary<DateTime, decimal>> TotalesDiariosAsync(DateTime desde, DateTime hasta)
        {
            using var db = Conn();

            var rows = await db.QueryAsync<(DateTime Dia, decimal Total)>(@"
SELECT CAST(Fecha AS date) AS Dia, SUM(Total) AS Total
FROM dbo.Ventas
WHERE Estado='Activa'
  AND Fecha >= @d
  AND Fecha < DATEADD(day,1,@h)
GROUP BY CAST(Fecha AS date)
ORDER BY Dia;",
                new { d = desde.Date, h = hasta.Date });

            return rows.ToDictionary(x => x.Dia.Date, x => x.Total);
        }
    }
}
