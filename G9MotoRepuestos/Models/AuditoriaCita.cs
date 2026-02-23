using System;

namespace G9MotoRepuestos.Models
{
    public class AuditoriaCita
    {
        public int Id { get; set; }
        public string Accion { get; set; }
        public int IdCita { get; set; }
        public string EstadoNuevo { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
