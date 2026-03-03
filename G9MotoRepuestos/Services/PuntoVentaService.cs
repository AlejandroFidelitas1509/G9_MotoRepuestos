
using Dapper;
using G9MotoRepuestos.Models.ViewModels;
using global::G9MotoRepuestos.Models.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace G9MotoRepuestos.Services;

public interface IPuntoVentaService
{
    Task<CartItemVm?> BuscarProductoAsync(string query);
    Task<(bool ok, string? error)> ValidarStockAsync(int idProducto, int cantidad);
    Task<int> CrearVentaAsync(int? idUsuario, string formaPago, decimal subtotal, decimal impuesto, decimal descuento, decimal total, IEnumerable<CartItemVm> items);
    Task<(bool ok, string? error)> AnularVentaAsync(int idVenta, int? idUsuario, string motivo);
}

public class PuntoVentaService : IPuntoVentaService
{
    private readonly string _cn;

    public PuntoVentaService(IConfiguration config)
    {
        _cn = config.GetConnectionString("DefaultConnection")!;
    }

    private IDbConnection Conn() => new SqlConnection(_cn);

    public async Task<CartItemVm?> BuscarProductoAsync(string query)
    {
        // Busca por código de barras o por nombre
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

    public async Task<(bool ok, string? error)> ValidarStockAsync(int idProducto, int cantidad)
    {
        const string sql = @"SELECT ISNULL(StockActual,0) FROM dbo.Inventario WHERE IdProducto = @id;";
        using var db = Conn();
        var stock = await db.ExecuteScalarAsync<int?>(sql, new { id = idProducto }) ?? 0;

        if (stock <= 0) return (false, "Producto no disponible en inventario");
        if (cantidad > stock) return (false, $"Stock insuficiente. Máximo disponible: {stock}");
        return (true, null);
    }

    public async Task<int> CrearVentaAsync(int? idUsuario, string formaPago, decimal subtotal, decimal impuesto, decimal descuento, decimal total, IEnumerable<CartItemVm> items)
    {
        using var db = Conn();
        using var tx = db.BeginTransaction();

        try
        {
            // 1) Insert Venta
            var idVenta = await db.ExecuteScalarAsync<int>(@"
INSERT INTO dbo.Ventas (Fecha, IdUsuario, FormaPago, Subtotal, Impuesto, Descuento, Total, Estado)
VALUES (SYSUTCDATETIME(), @IdUsuario, @FormaPago, @Subtotal, @Impuesto, @Descuento, @Total, 'Activa');
SELECT CAST(SCOPE_IDENTITY() AS int);",
                new { IdUsuario = idUsuario, FormaPago = formaPago, Subtotal = subtotal, Impuesto = impuesto, Descuento = descuento, Total = total },
                tx);

            // 2) Insert detalle + rebajar inventario
            foreach (var it in items)
            {
                // Validación stock real
                var okStock = await ValidarStockAsync(it.Id, it.Cantidad);
                if (!okStock.ok) throw new InvalidOperationException(okStock.error);

                await db.ExecuteAsync(@"
INSERT INTO dbo.VentaDetalle (IdVenta, IdProducto, NombreProducto, Cantidad, PrecioUnitario, SubtotalLinea)
VALUES (@IdVenta, @IdProducto, @Nombre, @Cantidad, @Precio, @SubtotalLinea);",
                    new
                    {
                        IdVenta = idVenta,
                        IdProducto = it.Id,
                        Nombre = it.Nombre,
                        Cantidad = it.Cantidad,
                        Precio = it.Precio,
                        SubtotalLinea = it.Precio * it.Cantidad
                    }, tx);

                await db.ExecuteAsync(@"
UPDATE dbo.Inventario
SET StockActual = StockActual - @Cant
WHERE IdProducto = @IdProducto;",
                    new { Cant = it.Cantidad, IdProducto = it.Id }, tx);
            }

            // 3) Registrar finanzas (ingreso) para cierres contables
            await db.ExecuteAsync(@"
INSERT INTO dbo.Finanzas (Tipo, Monto, Fecha, Categoria, Descripcion)
VALUES ('Ingreso', @Monto, CAST(GETDATE() AS date), 'Ventas', CONCAT('Venta #', @IdVenta));",
                new { Monto = total, IdVenta = idVenta }, tx);

            // 4) Auditoría (Issue 169)
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

    public async Task<(bool ok, string? error)> AnularVentaAsync(int idVenta, int? idUsuario, string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return (false, "Datos incompletos para hacer la devolución");

        using var db = Conn();
        using var tx = db.BeginTransaction();

        try
        {
            var estado = await db.ExecuteScalarAsync<string?>(@"SELECT Estado FROM dbo.Ventas WHERE IdVenta=@id;", new { id = idVenta }, tx);
            if (estado == null) return (false, "La factura no existe");
            if (estado == "Anulada") return (false, "La factura ya fue anulada");

            // Reponer inventario según detalle
            var detalle = (await db.QueryAsync<(int IdProducto, int Cantidad)>(
                @"SELECT IdProducto, Cantidad FROM dbo.VentaDetalle WHERE IdVenta=@id;", new { id = idVenta }, tx)).ToList();

            foreach (var d in detalle)
            {
                await db.ExecuteAsync(@"
UPDATE dbo.Inventario
SET StockActual = StockActual + @Cant
WHERE IdProducto = @IdProducto;",
                    new { Cant = d.Cantidad, IdProducto = d.IdProducto }, tx);
            }

            // Marcar venta como anulada
            await db.ExecuteAsync(@"UPDATE dbo.Ventas SET Estado='Anulada' WHERE IdVenta=@id;", new { id = idVenta }, tx);

            // Registrar anulación
            await db.ExecuteAsync(@"INSERT INTO dbo.AnulacionesVenta (IdVenta, Motivo) VALUES (@IdVenta, @Motivo);",
                new { IdVenta = idVenta, Motivo = motivo }, tx);

            // Reverso en finanzas (egreso)
            var total = await db.ExecuteScalarAsync<decimal>(@"SELECT Total FROM dbo.Ventas WHERE IdVenta=@id;", new { id = idVenta }, tx);
            await db.ExecuteAsync(@"
INSERT INTO dbo.Finanzas (Tipo, Monto, Fecha, Categoria, Descripcion)
VALUES ('Egreso', @Monto, CAST(GETDATE() AS date), 'Anulaciones', CONCAT('Anulación venta #', @IdVenta));",
                new { Monto = total, IdVenta = idVenta }, tx);

            // Auditoría
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
}
