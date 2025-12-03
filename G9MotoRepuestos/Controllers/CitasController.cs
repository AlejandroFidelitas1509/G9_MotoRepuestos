using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Models;

namespace G9MotoRepuestos.Controllers
{
    public class CitasController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult Details()
        {
            return View();
        }

        public IActionResult Edit()
        {
            return View();
        }
        public IActionResult historial()
        {
            return View();
        }
        public IActionResult BloquearFecha()
        {
            return View();
        }
    }
}