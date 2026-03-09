namespace G9MotoRepuestos.Models.ViewModels
{
    public class CartItemVm
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public int Stock { get; set; }

        public decimal TotalLinea => Precio * Cantidad;
    }
}
