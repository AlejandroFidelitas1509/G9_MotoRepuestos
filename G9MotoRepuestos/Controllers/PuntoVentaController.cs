using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Models.ViewModels;
using G9MotoRepuestos.Helpers;
using G9MotoRepuestos.Services;
using System.Security.Claims;
using G9MotoRepuestos.Data;
using Microsoft.EntityFrameworkCore;

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
                TempData["Error"] = "Productos o servicios fueron ingresados incorrectamente";
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
            TempData["Ok"] = "Producto/servicio agregado correctamente";
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
                TempData["Error"] = "No se pueden eliminar porque no existen";
                return RedirectToAction(nameof(Index));
            }

            cart.Remove(item);
            SaveCart(cart);
            TempData["Ok"] = "Producto/servicio eliminado correctamente";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            SaveCart(new List<CartItemVm>());
            TempData["Warning"] = "venta cancelada";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Cobrar()
        {
            var cart = GetCart();
            var vm = new PuntoVentaVm { Carrito = cart };

            if (!vm.Carrito.Any())
            {
                TempData["Error"] = "No se puede cobrar sin productos";
                return RedirectToAction(nameof(Index));
            }

            return View(vm);
        }

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
                return RedirectToAction(nameof(Cobrar));
            }

            if (vm.Total <= 0)
            {
                TempData["Error"] = "Error en cálculo de montos";
                return RedirectToAction(nameof(Cobrar));
            }

            if (formaPago == "Efectivo" && montoRecibido < vm.Total)
            {
                TempData["Error"] = "Monto recibido insuficiente";
                return RedirectToAction(nameof(Cobrar));
            }

            // ✅ Bloqueo por cierre contable
            if (await _db.Cierres.AnyAsync(c => DateTime.Today >= c.FechaInicio && DateTime.Today <= c.FechaFin))
            {
                TempData["Error"] = "No se puede registrar la venta: el período ya cuenta con un cierre contable.";
                return RedirectToAction(nameof(Index));
            }

            // Usuario (si existe autenticación)
            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? idUsuario = int.TryParse(idUsuarioStr, out var n) ? n : (int?)null;

            try
            {
                var idVenta = await _ventas.CrearVentaAsync(idUsuario, formaPago, vm.Subtotal, 0m, 0m, vm.Total, vm.Carrito);

                SaveCart(new List<CartItemVm>());
                TempData["Ok"] = "Factura generada exitosamente (venta exitosa)";
                return RedirectToAction(nameof(Factura), new { id = idVenta });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Cobrar));
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnularFactura(int idVenta, string motivo)
        {
            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? idUsuario = int.TryParse(idUsuarioStr, out var n) ? n : (int?)null;

            var res = await _ventas.AnularVentaAsync(idVenta, idUsuario, motivo);
            if (!res.ok)
            {
                TempData["Error"] = res.error;
                return RedirectToAction(nameof(Factura), new { id = idVenta });
            }

            TempData["Ok"] = "Factura anulada correctamente";
            return RedirectToAction(nameof(Factura), new { id = idVenta });
        }

        // ✅ Vista auditoría (Issue 169)
        [HttpGet]
        public async Task<IActionResult> Auditoria(DateTime? desde, DateTime? hasta, string? accion)
        {
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.Accion = accion ?? "";

            var data = await _ventas.AuditoriaAsync(desde, hasta, accion);
            return View(data);
        }

        private List<CartItemVm> GetCart()
            => HttpContext.Session.GetObject<List<CartItemVm>>(CART_KEY) ?? new List<CartItemVm>();

        private void SaveCart(List<CartItemVm> cart)
            => HttpContext.Session.SetObject(CART_KEY, cart);
    }
}
