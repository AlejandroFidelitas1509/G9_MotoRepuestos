using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Models.ViewModels;
using G9MotoRepuestos.Helpers;
using G9MotoRepuestos.Services;
using G9MotoRepuestos.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MR.Abstracciones.LogicaDeNegocio.Finanzas;

namespace G9MotoRepuestos.Controllers
{
    public class PuntoVentaController : Controller
    {
        private const string CART_KEY = "PV_CART";
        private readonly IVentasService _ventas;
        private readonly ApplicationDbContext _db;
        private readonly IFinanzasLN _finanzasLN;

        public PuntoVentaController(
            IVentasService ventas,
            ApplicationDbContext db,
            IFinanzasLN finanzasLN)
        {
            _ventas = ventas;
            _db = db;
            _finanzasLN = finanzasLN;
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(new PuntoVentaVm { Carrito = cart });
        }

        [HttpGet]
        public async Task<IActionResult> BuscarItems(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
                return Json(new List<object>());

            var items = await _ventas.BuscarSugerenciasAsync(term.Trim());

            var data = items.Select(x => new
            {
                id = x.Id,
                codigo = x.Codigo,
                nombre = x.Nombre,
                precio = x.Precio,
                stock = x.Stock,
                imagenUrl = x.ImagenUrl,
                tipo = x.Tipo
            });

            return Json(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Debe ingresar un código o nombre.";
                return RedirectToAction(nameof(Index));
            }

            var found = await _ventas.BuscarProductoAsync(query);
            if (found == null)
            {
                TempData["Error"] = "Producto o servicio no encontrado.";
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

            if (existing != null)
                existing.Cantidad++;
            else
                cart.Add(found);

            SaveCart(cart);
            TempData["Ok"] = $"{found.Tipo} agregado correctamente.";
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
                TempData["Error"] = "El elemento no existe en el carrito.";
                return RedirectToAction(nameof(Index));
            }

            if (qty <= 0)
            {
                TempData["Error"] = "Cantidad inválida.";
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

            TempData["Ok"] = "Cantidad actualizada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveItem(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.Id == id);

            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["Ok"] = "Elemento eliminado del carrito.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            SaveCart(new List<CartItemVm>());
            TempData["Warning"] = "Venta cancelada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(string formaPago, decimal montoRecibido)
        {
            var cart = GetCart();
            var vm = new PuntoVentaVm
            {
                Carrito = cart,
                FormaPago = formaPago,
                MontoRecibido = montoRecibido
            };

            if (!vm.Carrito.Any())
            {
                TempData["Error"] = "No hay productos o servicios para procesar.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(formaPago))
            {
                TempData["Error"] = "Debe seleccionar una forma de pago.";
                return RedirectToAction(nameof(Index));
            }

            if (formaPago.Equals("Efectivo", StringComparison.OrdinalIgnoreCase) && montoRecibido < vm.Total)
            {
                TempData["Error"] = "Monto recibido insuficiente.";
                return RedirectToAction(nameof(Index));
            }

            if (await _db.Cierres.AnyAsync(c => DateTime.Today >= c.FechaInicio && DateTime.Today <= c.FechaFin))
            {
                TempData["Error"] = "No se puede registrar la venta: el período está cerrado.";
                return RedirectToAction(nameof(Index));
            }

            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? idUsuario = int.TryParse(idUsuarioStr, out var n) ? n : (int?)null;

            if (!idUsuario.HasValue || idUsuario.Value <= 0)
            {
                TempData["Error"] = "No se pudo identificar el usuario actual.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var idVenta = await _ventas.CrearVentaAsync(
                    idUsuario,
                    formaPago,
                    vm.Subtotal,
                    vm.Iva,
                    0m,
                    vm.Total,
                    vm.Carrito
                );

                await _finanzasLN.RegistrarIngresoAutomaticoAsync(
                    vm.Total,
                    idUsuario.Value,
                    "Venta",
                    $"Venta #{idVenta} - Forma de pago: {formaPago}",
                    "Venta",
                    idVenta
                );

                SaveCart(new List<CartItemVm>());
                TempData["Ok"] = "Compra exitosa ✅";
                TempData["UltimaVentaId"] = idVenta;

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private List<CartItemVm> GetCart()
            => HttpContext.Session.GetObject<List<CartItemVm>>(CART_KEY) ?? new List<CartItemVm>();

        private void SaveCart(List<CartItemVm> cart)
            => HttpContext.Session.SetObject(CART_KEY, cart);
    }
}
