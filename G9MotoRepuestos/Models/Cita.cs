using System;

namespace G9MotoRepuestos.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; }
        public string Descripcion { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = "Pendiente";
    }
}