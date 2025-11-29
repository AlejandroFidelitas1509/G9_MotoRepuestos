using Microsoft.AspNetCore.Mvc;

namespace G9MotoRepuestos.Controllers
{
    public class ContabilidadController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Cierres()
        {
            return View();
        }

        // 👉 Nueva acción para recibir los datos del cierre
        [HttpGet]
        public IActionResult CierreResultado(string tipo, DateTime? fecha, DateTime? fechaInicio, DateTime? fechaFin)
        {
            // Pasamos los datos a la vista
            ViewBag.Tipo = tipo;
            ViewBag.Fecha = fecha;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View(); // Busca Views/Contabilidad/CierreResultado.cshtml
        }
    }
}
