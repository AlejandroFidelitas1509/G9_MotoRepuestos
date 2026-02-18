using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Models.ViewModels;
using G9MotoRepuestos.Helpers;

namespace G9MotoRepuestos.Controllers
{
    public class PuntoVentaController : Controller
    {
        private const string CART_KEY = "PV_CART";

        // ✅ POS principal
        public IActionResult Index()
        {
            var cart = GetCart();
            var vm = new PuntoVentaVm { Carrito = cart };
            return View(vm);
        }

        // ✅ Agregar producto por código o nombre (PV-001 / PV-005 parte 1)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddItem(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Faltan espacios por completar";
                return RedirectToAction(nameof(Index));
            }

            // TODO: aquí conectamos con tu capa MR para buscar por código/nombre.
            // Por ahora, un ejemplo funcional:
            var found = FakeFindProducto(query);

            if (found == null)
            {
                TempData["Error"] = "Productos o servicios fueron ingresados incorrectamente";
                return RedirectToAction(nameof(Index));
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(x => x.Id == found.Id);

            if (existing != null)
            {
                // aumenta cantidad con límite stock
                if (found.Stock > 0 && existing.Cantidad + 1 > found.Stock)
                {
                    TempData["Error"] = "Stock insuficiente";
                    return RedirectToAction(nameof(Index));
                }
                existing.Cantidad += 1;
            }
            else
            {
                cart.Add(found);
            }

            SaveCart(cart);
            TempData["Ok"] = "Producto/servicio agregado correctamente";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Actualizar cantidad (PV-003 / validaciones PV-006)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQty(int id, int qty)
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

            if (item.Stock > 0 && qty > item.Stock)
            {
                TempData["Error"] = "Stock insuficiente";
                return RedirectToAction(nameof(Index));
            }

            item.Cantidad = qty;
            SaveCart(cart);
            TempData["Ok"] = "Cantidad actualizada";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Eliminar item (PV-004)
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

        // ✅ Cancelar operación (PV-005 escenario 2)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            SaveCart(new List<CartItemVm>());
            TempData["Warning"] = "venta cancelada";
            return RedirectToAction(nameof(Index));
        }

        // ✅ Ir a Cobrar (F10)
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

        // ✅ Finalizar (PV-006 + PV-005 éxito + PV-008 bitácora después)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Finalizar(string formaPago, decimal montoRecibido)
        {
            var cart = GetCart();
            var vm = new PuntoVentaVm { Carrito = cart, FormaPago = formaPago, MontoRecibido = montoRecibido };

            if (!vm.Carrito.Any())
            {
                TempData["Error"] = "No se puede generar una factura sin productos";
                return RedirectToAction(nameof(Index));
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

            // TODO: aquí insertamos Venta + Factura + Bitácora en BD (PV-006 y PV-008).
            // Por ahora: simulación exitosa.
            SaveCart(new List<CartItemVm>());

            TempData["Ok"] = "Factura generada exitosamente (venta exitosa)";
            return RedirectToAction(nameof(Index));
        }

        // -------------------------
        // Session helpers
        // -------------------------
        private List<CartItemVm> GetCart()
            => HttpContext.Session.GetObject<List<CartItemVm>>(CART_KEY) ?? new List<CartItemVm>();

        private void SaveCart(List<CartItemVm> cart)
            => HttpContext.Session.SetObject(CART_KEY, cart);

        // -------------------------
        // MOCK (reemplazar por tu BD/MR)
        // -------------------------
        private CartItemVm? FakeFindProducto(string query)
        {
            // Simula buscar por código o nombre
            // Cambiá esto por tu lógica real
            if (query.Trim() == "744102" || query.ToLower().Contains("kit"))
            {
                return new CartItemVm
                {
                    Id = 1,
                    Codigo = "744102",
                    Nombre = "Kit de Arrastre Racing",
                    Precio = 25500m,
                    Stock = 12,
                    Cantidad = 1
                };
            }
            return null;
        }
    }
}

