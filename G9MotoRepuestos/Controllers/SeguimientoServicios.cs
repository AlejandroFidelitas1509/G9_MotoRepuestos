using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace G9MotoRepuestos.Controllers
{
    public class SeguimientoServiciosController : Controller
    {
       
        public static List<Servicio> Servicios = new List<Servicio>()
        {
            new Servicio { Id = 1, Nombre = "Cambio de aceite", Descripcion = "Mantenimiento básico", Precio = 20 }
        };

        public static List<Bitacora> Bitacoras = new List<Bitacora>()
        {
            new Bitacora { Accion = "Creación", IdServicio = 1, FechaHora = DateTime.Now }
        };

      
        public IActionResult Index()
        {
            return View("Servicios");
        }

        [HttpPost]
        public IActionResult CrearServicio(string Nombre, string Descripcion, decimal Precio)
        {
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Descripcion))
            {
                ViewBag.Mensaje = "Faltan campos por completar.";
                return View("Servicios");
            }

            int newId = Servicios.Count + 1;

            Servicios.Add(new Servicio
            {
                Id = newId,
                Nombre = Nombre,
                Descripcion = Descripcion,
                Precio = Precio
            });

            Bitacoras.Add(new Bitacora
            {
                Accion = "Creación",
                IdServicio = newId,
                FechaHora = DateTime.Now
            });

            ViewBag.Mensaje = "Servicio agregado correctamente.";
            return View("Servicios");
        }

     
        [HttpPost]
        public IActionResult EditarServicio(int Id, string Nombre, string Descripcion, decimal Precio)
        {
            var servicio = Servicios.FirstOrDefault(s => s.Id == Id);

            if (servicio == null)
            {
                ViewBag.Mensaje = "El servicio no existe.";
                return View("Servicios");
            }

            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Descripcion))
            {
                ViewBag.Mensaje = "Hay errores de validación.";
                return View("Servicios");
            }

            servicio.Nombre = Nombre;
            servicio.Descripcion = Descripcion;
            servicio.Precio = Precio;

            Bitacoras.Add(new Bitacora
            {
                Accion = "Edición",
                IdServicio = Id,
                FechaHora = DateTime.Now
            });

            ViewBag.Mensaje = "Servicio actualizado correctamente.";
            return View("Servicios");
        }

    
        [HttpPost]
        public IActionResult EliminarServicio(int Id)
        {
            var servicio = Servicios.FirstOrDefault(s => s.Id == Id);

            if (servicio == null)
            {
                ViewBag.Mensaje = "El servicio no fue encontrado.";
                return View("Servicios");
            }

            Servicios.Remove(servicio);

            Bitacoras.Add(new Bitacora
            {
                Accion = "Eliminación",
                IdServicio = Id,
                FechaHora = DateTime.Now
            });

            ViewBag.Mensaje = "Servicio eliminado correctamente.";
            return View("Servicios");
        }

        
        [HttpPost]
        public IActionResult RegistrarDiagnostico(int IdServicio, string Diagnostico)
        {
            if (string.IsNullOrWhiteSpace(Diagnostico))
            {
                ViewBag.Mensaje = "Faltan datos en el diagnóstico.";
                return View("Servicios");
            }

            ViewBag.Mensaje = $"Diagnóstico guardado para el servicio #{IdServicio}.";
            return View("Servicios");
        }

      
        [HttpPost]
        public IActionResult ConsultarEstado(int IdServicio)
        {
            var servicio = Servicios.FirstOrDefault(s => s.Id == IdServicio);

            if (servicio == null)
            {
                ViewBag.Mensaje = "No hay servicios disponibles.";
                return View("Servicios");
            }

            ViewBag.Mensaje = $"Estado actual del servicio #{IdServicio}: EN PROCESO.";
            return View("Servicios");
        }
    }


    
    public class Servicio
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }

    public class Bitacora
    {
        public string Accion { get; set; }
        public int IdServicio { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
