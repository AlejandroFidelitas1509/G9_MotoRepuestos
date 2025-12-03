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

        public IActionResult Historial()
        {
            return View();
        }


        [HttpGet]
        public IActionResult CierreResultado(string tipo, DateTime? fecha, DateTime? fechaInicio, DateTime? fechaFin)
        {
         
            ViewBag.Tipo = tipo;
            ViewBag.Fecha = fecha;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            return View();

        }
    }
}


