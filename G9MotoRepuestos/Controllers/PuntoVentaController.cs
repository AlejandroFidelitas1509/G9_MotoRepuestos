using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Models.ViewModels;
using G9MotoRepuestos.Helpers;
using G9MotoRepuestos.Services;
using System.Security.Claims;
using G9MotoRepuestos.Data;
using Microsoft.EntityFrameworkCore;

// PDF
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace G9MotoRepuestos.Controllers
{
    public class PuntoVentaController : Controller
    {
        private const string CART_KEY = "PV_CART";
        private readonly IVentasService _ventas;
        private readonly ApplicationDbContext _db;

        public PuntoVentaController(IVentasService ventas, ApplicationDbContext db)
        {
            _ventas = ventas;
            _db = db;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(new PuntoVentaVm { Carrito = cart });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Faltan espacios por completar";
                return RedirectToAction(nameof(Index));
            }

            var found = await _ventas.BuscarProductoAsync(query);
            if (found == null)
            {
                TempData["Error"] = "Producto no encontrado";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.Id == found.Id);

            var nuevaCantidad = (existing?.Cantidad ?? 0) + 1;
            var stockOk = await _ventas.ValidarStockAsync(found.Id, nuevaCantidad);
            if (!stockOk.ok)
            {
                TempData["Error"] = stockOk.error;
                return RedirectToAction(nameof(Index));
            }

            if (existing != null) existing.Cantidad++;
            else cart.Add(found);

            SaveCart(cart);
            TempData["Ok"] = "Producto agregado";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQty(int id, int qty)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                TempData["Error"] = "El producto no existe en el carrito";
                return RedirectToAction(nameof(Index));
            }

            if (qty <= 0)
            {
                TempData["Error"] = "Cantidad inválida";
                return RedirectToAction(nameof(Index));
            }

            var stockOk = await _ventas.ValidarStockAsync(id, qty);
            if (!stockOk.ok)
            {
                TempData["Error"] = stockOk.error;
                return RedirectToAction(nameof(Index));
            }

            item.Cantidad = qty;
            SaveCart(cart);
            TempData["Ok"] = "Cantidad actualizada";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Id == id);
            if (item == null)
            {
                TempData["Error"] = "El producto no existe";
                return RedirectToAction(nameof(Index));
            }

            cart.Remove(item);
            SaveCart(cart);
            TempData["Ok"] = "Producto eliminado";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            SaveCart(new List<CartItemVm>());
            TempData["Warning"] = "Operación cancelada";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Finalizar venta (abre PDF en nueva pestaña y vuelve al POS)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(string formaPago, decimal montoRecibido)
        {
            var cart = GetCart();
            var vm = new PuntoVentaVm { Carrito = cart, FormaPago = formaPago, MontoRecibido = montoRecibido };

            if (!vm.Carrito.Any())
            {
                TempData["Error"] = "No se puede generar una factura sin productos";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(formaPago))
            {
                TempData["Error"] = "Debe seleccionar una forma de pago";
                return RedirectToAction(nameof(Index));
            }

            formaPago = formaPago.Trim();

            if (vm.Total <= 0)
            {
                TempData["Error"] = "Error en cálculo de montos";
                return RedirectToAction(nameof(Index));
            }

            if (formaPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) && montoRecibido < vm.Total)
            {
                TempData["Error"] = "Monto recibido insuficiente";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Bloqueo por cierre contable (si cae dentro del período)
            var hoy = DateTime.Today;
            if (await _db.Cierres.AnyAsync(c => hoy >= c.FechaInicio && hoy <= c.FechaFin))
            {
                TempData["Error"] = "No se puede registrar la venta: el período ya cuenta con un cierre contable.";
                return RedirectToAction(nameof(Index));
            }


            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? idUsuario = int.TryParse(idUsuarioStr, out var n) ? n : (int?)null;

            try
            {
                // ✅ crea venta y rebaja inventario (en VentasService)
                var idVenta = await _ventas.CrearVentaAsync(
                    idUsuario,
                    formaPago,
                    vm.Subtotal,
                    0m,
                    0m,
                    vm.Total,
                    vm.Carrito
                );

                // ✅ limpiar carrito para nueva compra
                SaveCart(new List<CartItemVm>());

                // ✅ Mensaje
                TempData["Ok"] = "Compra exitosa ✅";

                // ✅ Paso extra: devolver una vista que abre el PDF en nueva pestaña y regresa al POS
                return View("PostFinalizar", idVenta);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Factura(int id)
        {
            var factura = await _ventas.ObtenerFacturaAsync(id);
            if (factura == null)
            {
                TempData["Error"] = "Factura no encontrada";
                return RedirectToAction(nameof(Index));
            }
            return View(factura);
        }

        [HttpGet]
        public async Task<IActionResult> FacturaPdf(int id)
        {
            var factura = await _ventas.ObtenerFacturaAsync(id);
            if (factura == null) return NotFound();

            var doc = new FacturaPdfDocument(factura);
            var pdfBytes = doc.GeneratePdf();

            return File(pdfBytes, "application/pdf", $"Factura_{factura.IdVenta}.pdf");
        }

        private List<CartItemVm> GetCart()
            => HttpContext.Session.GetObject<List<CartItemVm>>(CART_KEY) ?? new List<CartItemVm>();

        private void SaveCart(List<CartItemVm> cart)
            => HttpContext.Session.SetObject(CART_KEY, cart);
    }

    // ==========================
    // ✅ PDF DOCUMENT (QuestPDF)
    // ==========================
    internal class FacturaPdfDocument : IDocument
    {
        private readonly FacturaVm _m;
        public FacturaPdfDocument(FacturaVm model) => _m = model;

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Moto Repuestos Rojas - ");
                    x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeColumn().Stack(stack =>
                {
                    stack.Item().Text("FACTURA").FontSize(20).Bold();
                    stack.Item().Text($"N° Venta: {_m.IdVenta}");
                    stack.Item().Text($"Fecha: {_m.Fecha:yyyy-MM-dd HH:mm}");
                    stack.Item().Text($"Forma de pago: {_m.FormaPago}");
                    stack.Item().Text($"Estado: {_m.Estado}");
                });

                row.ConstantColumn(180).AlignRight().Stack(stack =>
                {
                    stack.Item().Text("Moto Repuestos Rojas").Bold();
                    stack.Item().Text("Punto de Venta");
                    stack.Item().Text("Costa Rica");
                });
            });

            container.PaddingTop(10).LineHorizontal(1);
        }

        private void ComposeContent(IContainer container)
        {
            container.Stack(stack =>
            {
                stack.Item().PaddingTop(10).Text("Detalle").Bold().FontSize(14);

                stack.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(70);
                        cols.RelativeColumn();
                        cols.ConstantColumn(50);
                        cols.ConstantColumn(80);
                        cols.ConstantColumn(90);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyleHeader).Text("Cod.");
                        header.Cell().Element(CellStyleHeader).Text("Descripción");
                        header.Cell().Element(CellStyleHeader).AlignCenter().Text("Cant.");
                        header.Cell().Element(CellStyleHeader).AlignRight().Text("Precio");
                        header.Cell().Element(CellStyleHeader).AlignRight().Text("Total");
                    });

                    foreach (var d in _m.Detalle)
                    {
                        table.Cell().Element(CellStyle).Text(d.Codigo);
                        table.Cell().Element(CellStyle).Text(d.NombreProducto);
                        table.Cell().Element(CellStyle).AlignCenter().Text(d.Cantidad.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text($"₡ {d.PrecioUnitario:N2}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"₡ {d.SubtotalLinea:N2}");
                    }
                });

                stack.Item().PaddingTop(15).AlignRight().Stack(t =>
                {
                    t.Item().Text($"Subtotal: ₡ {_m.Subtotal:N2}");
                    t.Item().Text($"IVA: ₡ {_m.Impuesto:N2}");
                    t.Item().Text($"Descuento: ₡ {_m.Descuento:N2}");
                    t.Item().Text($"TOTAL: ₡ {_m.Total:N2}").Bold().FontSize(14);
                });
            });
        }

        private static IContainer CellStyleHeader(IContainer container)
        {
            return container
                .DefaultTextStyle(x => x.Bold().FontColor(Colors.White))
                .Background(Colors.Black)
                .PaddingVertical(6)
                .PaddingHorizontal(6);
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(6);
        }
    }
}
