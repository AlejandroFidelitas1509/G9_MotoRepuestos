using Microsoft.AspNetCore.Mvc;

namespace G9MotoRepuestos.Controllers
{
    public class UsuariosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Acción para mostrar el formulario de creación
        public IActionResult Create()
        {
            return View();
        }


        // Acción para mostrar el formulario de edición
        public IActionResult Edit()
        {
            return View();
        }

        // Acción para mostrar la confirmación de eliminación
        public IActionResult Delete()
        {
            return View();
        }




    }
}
