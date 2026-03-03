namespace G9MotoRepuestos.Models.ViewModels
{
    public class FacturaVm
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string FormaPago { get; set; } = "";
        public decimal Subtotal { get; set; }
        public decimal Impuesto { get; set; }
        public decimal Descuento { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = "Activa";
        public List<FacturaLineaVm> Detalle { get; set; } = new();
    }

    public class FacturaLineaVm
    {
        public string Codigo { get; set; } = "";
        public string NombreProducto { get; set; } = "";
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalLinea { get; set; }
    }
}
