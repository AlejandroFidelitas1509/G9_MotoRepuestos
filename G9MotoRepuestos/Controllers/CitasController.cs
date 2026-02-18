using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Models;
using MR.LogicaNegocio.Servicios;
using MR.LogicaNegocio.Dtos;

namespace G9MotoRepuestos.Controllers
{
    public class CitasController : Controller
    {

        private readonly ILogger<CitasController> _logger;

        private readonly ICitasServicio _citasServicio;

        public CitasController(ILogger<CitasController> logger, ICitasServicio citasServicio)
        {
            _logger = logger;
            _citasServicio = citasServicio;
        }

        public async Task<IActionResult> Index()
        {
            var respuesta = await _citasServicio.ObtenerCitasAsync();
            return View(respuesta.Data);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CitasDto citaDto)
        {
            if (ModelState.IsValid)
            {
                var respuesta = await _citasServicio.AgregarCitaAsync(citaDto);
                if (!respuesta.EsError)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(string.Empty, respuesta.Mensaje);
            }
            return View(citaDto);



        }
    }
}