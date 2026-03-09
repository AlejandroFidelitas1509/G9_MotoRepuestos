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

        Task<int> CrearVentaAsync(
            int? idUsuario,
            string formaPago,
            decimal subtotal,
            decimal impuesto,
            decimal descuento,
            decimal total,
            IEnumerable<CartItemVm> items);

        Task<FacturaVm?> ObtenerFacturaAsync(int idVenta);
        Task<Dictionary<DateTime, decimal>> TotalesDiariosAsync(DateTime desde, DateTime hasta);
    }

    public class VentasService : IVentasService
    {
        private readonly string _cn;

        public VentasService(IConfiguration config)
        {
            _cn = config.GetConnectionString("DefaultConnection")!;
        }

        // Buscar producto por código o nombre
        public async Task<CartItemVm?> BuscarProductoAsync(string query)
        {
            await using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();

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
  AND ISNULL(p.Estado, 0) = 1
ORDER BY p.IdProducto DESC;";

            var item = await conn.QueryFirstOrDefaultAsync<CartItemVm>(sql, new { q = (query ?? "").Trim() });

            if (item == null)
                return null;

            item.Cantidad = 1;
            return item;
        }

        // Validar stock real
        public async Task<(bool ok, string? error)> ValidarStockAsync(int idProducto, int cantidad)
        {
            await using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();

            const string sql = @"
SELECT ISNULL(StockActual, 0)
FROM dbo.Inventario
WHERE IdProducto = @id;";

            var stock = await conn.ExecuteScalarAsync<int?>(sql, new { id = idProducto }) ?? 0;

            if (stock <= 0)
                return (false, "Producto no disponible en inventario");

            if (cantidad > stock)
                return (false, $"Stock insuficiente. Máximo disponible: {stock}");

            return (true, null);
        }

        // Crear venta + detalle + rebajar inventario
        public async Task<int> CrearVentaAsync(
            int? idUsuario,
            string formaPago,
            decimal subtotal,
            decimal impuesto,
            decimal descuento,
            decimal total,
            IEnumerable<CartItemVm> items)
        {
            await using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            try
            {
                // 1) Insertar encabezado de venta
                var idVenta = await conn.ExecuteScalarAsync<int>(@"
INSERT INTO dbo.Ventas (Fecha, IdUsuario, MetodoPago, SubTotal, IVA, Total)
VALUES (GETDATE(), @IdUsuario, @MetodoPago, @SubTotal, @IVA, @Total);
SELECT CAST(SCOPE_IDENTITY() AS int);",
                    new
                    {
                        IdUsuario = idUsuario,
                        MetodoPago = formaPago,
                        SubTotal = subtotal,
                        IVA = impuesto,
                        Total = total
                    }, tx);

                // 2) Insertar detalle y rebajar inventario usando la CANTIDAD REAL
                foreach (var it in items)
                {
                    var stock = await conn.ExecuteScalarAsync<int?>(@"
SELECT ISNULL(StockActual, 0)
FROM dbo.Inventario WITH (UPDLOCK, ROWLOCK)
WHERE IdProducto = @IdProducto;",
                        new { IdProducto = it.Id }, tx) ?? 0;

                    if (stock <= 0)
                        throw new InvalidOperationException("Producto no disponible en inventario");

                    if (it.Cantidad > stock)
                        throw new InvalidOperationException($"Stock insuficiente. Máximo disponible: {stock}");

                    await conn.ExecuteAsync(@"
INSERT INTO dbo.VentaDetalle
(IdVenta, IdProducto, NombreProducto, Cantidad, PrecioUnitario, SubTotalLinea)
VALUES
(@IdVenta, @IdProducto, @NombreProducto, @Cantidad, @PrecioUnitario, @SubTotalLinea);",
                        new
                        {
                            IdVenta = idVenta,
                            IdProducto = it.Id,
                            NombreProducto = it.Nombre,
                            Cantidad = it.Cantidad, // ✅ usa la cantidad real
                            PrecioUnitario = it.Precio,
                            SubTotalLinea = it.Precio * it.Cantidad
                        }, tx);

                    var rows = await conn.ExecuteAsync(@"
UPDATE dbo.Inventario
SET StockActual = StockActual - @Cant
WHERE IdProducto = @IdProducto;",
                        new
                        {
                            Cant = it.Cantidad, // ✅ rebaja la cantidad real
                            IdProducto = it.Id
                        }, tx);

                    if (rows == 0)
                        throw new InvalidOperationException("No existe registro de inventario para este producto.");
                }

                // 3) Registrar en finanzas si existe la tabla
                await conn.ExecuteAsync(@"
IF OBJECT_ID('dbo.Finanzas', 'U') IS NOT NULL
BEGIN
    INSERT INTO dbo.Finanzas (Tipo, Monto, Fecha, Descripcion)
    VALUES ('Ingreso', @Monto, GETDATE(), CONCAT('Venta #', @IdVenta));
END",
                    new { Monto = total, IdVenta = idVenta }, tx);

                await tx.CommitAsync();
                return idVenta;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        // Obtener factura
        public async Task<FacturaVm?> ObtenerFacturaAsync(int idVenta)
        {
            await using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();

            var header = await conn.QueryFirstOrDefaultAsync<FacturaVm>(@"
SELECT
    v.IdVenta,
    v.Fecha,
    v.MetodoPago AS FormaPago,
    v.SubTotal AS Subtotal,
    v.IVA AS Impuesto,
    CAST(0 AS decimal(18,2)) AS Descuento,
    v.Total
FROM dbo.Ventas v
WHERE v.IdVenta = @id;", new { id = idVenta });

            if (header == null)
                return null;

            var detalle = (await conn.QueryAsync<FacturaLineaVm>(@"
SELECT
    p.CodigoBarras AS Codigo,
    d.NombreProducto,
    d.Cantidad,
    d.PrecioUnitario,
    d.SubTotalLinea
FROM dbo.VentaDetalle d
LEFT JOIN dbo.Productos p ON p.IdProducto = d.IdProducto
WHERE d.IdVenta = @id;", new { id = idVenta })).ToList();

            header.Detalle = detalle;
            return header;
        }

        // Totales por día
        public async Task<Dictionary<DateTime, decimal>> TotalesDiariosAsync(DateTime desde, DateTime hasta)
        {
            await using var conn = new SqlConnection(_cn);
            await conn.OpenAsync();

            var rows = await conn.QueryAsync<(DateTime Dia, decimal Total)>(@"
SELECT CAST(Fecha AS date) AS Dia, SUM(Total) AS Total
FROM dbo.Ventas
WHERE Fecha >= @d AND Fecha < DATEADD(day, 1, @h)
GROUP BY CAST(Fecha AS date)
ORDER BY Dia;",
                new { d = desde.Date, h = hasta.Date });

            return rows.ToDictionary(x => x.Dia.Date, x => x.Total);
        }
    }
}