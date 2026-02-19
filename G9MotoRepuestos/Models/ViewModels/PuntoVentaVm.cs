namespace G9MotoRepuestos.Models.ViewModels
{
    public class PuntoVentaVm
    {
        public List<CartItemVm> Carrito { get; set; } = new();

        public decimal Subtotal => Carrito.Sum(x => x.TotalLinea);
        public decimal IvaRate { get; set; } = 0.13m;
        public decimal Iva => Math.Round(Subtotal * IvaRate, 2);
        public decimal Total => Subtotal + Iva;

        // Cobro9
        public string FormaPago { get; set; } = "Efectivo"; // Efectivo/Tarjeta/SINPE
        public decimal MontoRecibido { get; set; }
        public decimal Vuelto => MontoRecibido <= 0 ? 0 : Math.Max(0, MontoRecibido - Total);
    }
}
