using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Finanzas;

namespace G9MotoRepuestos.Controllers
{
    [Authorize]
    public class ContabilidadController : Controller
    {
        private readonly IFinanzasLN _finanzasLN;

        public ContabilidadController(IFinanzasLN finanzasLN)
        {
            _finanzasLN = finanzasLN;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? textoBusqueda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoFiltro,
            string? origenFiltro)
        {
            var modelo = new ContabilidadIndexDto
            {
                TextoBusqueda = textoBusqueda,
                Desde = desde,
                Hasta = hasta,
                TipoFiltro = tipoFiltro,
                OrigenFiltro = origenFiltro,
                Resumen = await _finanzasLN.ObtenerResumenAsync(desde, hasta, tipoFiltro, origenFiltro),
                Movimientos = (await _finanzasLN.ObtenerMovimientosAsync(
                    textoBusqueda,
                    desde,
                    hasta,
                    tipoFiltro,
                    origenFiltro)).ToList()
            };

            return View(modelo);
        }

        [HttpGet]
        public IActionResult RegistrarMovimiento()
        {
            var modelo = new MovimientoFinancieroDto
            {
                Fecha = DateTime.Now,
                Origen = "Manual"
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarMovimiento(MovimientoFinancieroDto modelo)
        {
            try
            {
                modelo.IdUsuario = ObtenerIdUsuarioActual();

                if (string.IsNullOrWhiteSpace(modelo.Origen))
                    modelo.Origen = "Manual";

                await _finanzasLN.RegistrarMovimientoAsync(modelo);

                TempData["Success"] = "El movimiento se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(modelo);
            }
        }

        [HttpGet]
        public IActionResult RealizarCierre(DateTime? desde, DateTime? hasta)
        {
            var hoy = DateTime.Today;

            var modelo = new CierreContableDto
            {
                FechaInicio = desde ?? hoy,
                FechaFin = hasta ?? hoy,
                FechaRegistro = DateTime.Now,
                Tipo = "Diario"
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RealizarCierre(CierreContableDto modelo)
        {
            try
            {
                modelo.IdUsuario = ObtenerIdUsuarioActual();

                await _finanzasLN.RegistrarCierreAsync(modelo);

                TempData["Success"] = "El cierre contable se registró correctamente.";
                return RedirectToAction(nameof(HistorialCierres));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return View(modelo);
            }
        }

        [HttpGet]
        public async Task<IActionResult> HistorialCierres()
        {
            var cierres = await _finanzasLN.ObtenerCierresAsync();
            return View(cierres);
        }

        private int ObtenerIdUsuarioActual()
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("IdUsuario")?.Value ??
                User.FindFirst("id")?.Value;

            if (int.TryParse(claim, out int idUsuario))
                return idUsuario;

            return 0;
        }
    }
}