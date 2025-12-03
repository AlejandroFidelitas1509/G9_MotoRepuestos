using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Models;

namespace G9MotoRepuestos.Controllers
{
    public class CitasController : Controller
    {
        private static List<Cita> citas = new List<Cita>();
        private static List<FechaBloqueada> fechasBloqueadas = new List<FechaBloqueada>();
        private static List<AuditoriaCita> auditoria = new List<AuditoriaCita>();

        public IActionResult Index()
        {
            return View(citas);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Cita cita)
        {
            if (!ModelState.IsValid)
                return View(cita);

            // Validar fecha bloqueada
            if (fechasBloqueadas.Any(f => f.Fecha.Date == cita.Fecha.Date))
            {
                ViewBag.Error = "La fecha seleccionada está bloqueada.";
                return View(cita);
            }

            // Validación de cita duplicada
            if (citas.Any(c => c.Fecha == cita.Fecha))
            {
                ViewBag.Error = "Ya existe una cita en esa fecha y hora.";
                return View(cita);
            }

            cita.Id = citas.Count + 1;
            citas.Add(cita);

            // AUDITORÍA
            auditoria.Add(new AuditoriaCita
            {
                Accion = "Crear",
                IdCita = cita.Id,
                FechaHora = DateTime.Now
            });

            TempData["Mensaje"] = "Cita creada exitosamente.";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var cita = citas.FirstOrDefault(c => c.Id == id);
            return View(cita);
        }

        [HttpPost]
        public IActionResult Edit(Cita cita)
        {
            var existente = citas.FirstOrDefault(c => c.Id == cita.Id);

            // Validación de conflicto de horario
            if (citas.Any(c => c.Fecha == cita.Fecha && c.Id != cita.Id))
            {
                ViewBag.Error = "Ya existe una cita en esa fecha y hora.";
                return View(cita);
            }

            existente.NombreCliente = cita.NombreCliente;
            existente.Descripcion = cita.Descripcion;
            existente.Fecha = cita.Fecha;

            // AUDITORÍA
            auditoria.Add(new AuditoriaCita
            {
                Accion = "Editar",
                IdCita = cita.Id,
                FechaHora = DateTime.Now
            });

            TempData["Mensaje"] = "Cita modificada correctamente.";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var cita = citas.FirstOrDefault(c => c.Id == id);
            return View(cita);
        }

        [HttpPost]
        public IActionResult ConfirmDelete(int id)
        {
            var cita = citas.FirstOrDefault(c => c.Id == id);
            citas.Remove(cita);

            auditoria.Add(new AuditoriaCita
            {
                Accion = "Eliminar",
                IdCita = id,
                FechaHora = DateTime.Now
            });

            return RedirectToAction("Index");
        }

        public IActionResult BloquearFecha()
        {
            return View(fechasBloqueadas);
        }

        [HttpPost]
        public IActionResult BloquearFecha(DateTime fecha)
        {
            if (fechasBloqueadas.Any(f => f.Fecha.Date == fecha.Date))
            {
                TempData["Mensaje"] = "La fecha ya está bloqueada.";
                return RedirectToAction("BloquearFecha");
            }

            fechasBloqueadas.Add(new FechaBloqueada
            {
                Id = fechasBloqueadas.Count + 1,
                Fecha = fecha
            });

            return RedirectToAction("BloquearFecha");
        }
    }
}