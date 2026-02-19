using AutoMapper;
using G9MotoRepuestos.Models;
using Microsoft.AspNetCore.Mvc;
using MR.AccesoDatos.Entidades;
using MR.AccesoDatos.Repositorios;
using MR.LogicaNegocio.Dtos;
using MR.LogicaNegocio.Servicios;

namespace G9MotoRepuestos.Controllers
{
    public class CitasController : Controller
    {

        private readonly ILogger<CitasController> _logger;

        private readonly ICitasServicio _citasServicio;
        private readonly ICitasRepositorio _citasRepositorio;
        private readonly IMapper _mapper;


        public CitasController(ILogger<CitasController> logger, ICitasServicio citasServicio, ICitasRepositorio citasRepositorio,
            IMapper mapper)
        {
            _logger = logger;
            _citasServicio = citasServicio;
            _citasRepositorio = citasRepositorio;
            _mapper = mapper;
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


        public async Task<IActionResult> Edit(int id)
        {
            var cita = await _citasRepositorio.ObtenerCitaPorIdAsync(id);
            if (cita == null)
            {
                return NotFound();
            }

            var citaDto = _mapper.Map<CitasDto>(cita);
            return View(citaDto);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CitasDto citaDto)
        {
            if (ModelState.IsValid)
            {
                var citaEntity = _mapper.Map<Citas>(citaDto);
                var actualizado = await _citasRepositorio.ActualizarCitaAsync(citaEntity);

                if (actualizado)
                    return RedirectToAction(nameof(Index));
            }

            return View(citaDto);

        }


        public async Task<IActionResult> Delete(int id)
        {
            var cita = await _citasRepositorio.ObtenerCitaPorIdAsync(id);
            if (cita == null)
            {
                return NotFound();
            }
            var citaDto = _mapper.Map<CitasDto>(cita);
            return View(citaDto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eliminado = await _citasRepositorio.EliminarCitaAsync(id);

            if (eliminado)
            {
                // Si se eliminó correctamente, redirige al listado
                return RedirectToAction(nameof(Index));
            }

            // Si no se pudo eliminar, muestra un error
            TempData["ErrorMessage"] = "No se pudo eliminar la cita.";
            return Problem("No se pudo eliminar la cita.");



        }


    }
}