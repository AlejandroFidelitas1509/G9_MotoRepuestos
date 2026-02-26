namespace G9MotoRepuestos.Models.ViewModels
{
    public class AuditoriaVentasVm
    {
        public int IdAuditoria { get; set; }
        public string Accion { get; set; } = "";
        public int? IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public int? IdUsuario { get; set; }
        public string? Descripcion { get; set; }
    }
}
