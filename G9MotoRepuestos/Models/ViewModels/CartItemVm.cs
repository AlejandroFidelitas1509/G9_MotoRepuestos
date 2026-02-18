namespace G9MotoRepuestos.Models.ViewModels
{
    public class CartItemVm
    {
        public int Id { get; set; }                 // Id del producto/servicio
        public string Codigo { get; set; } = "";    // código/barra si existe
        public string Nombre { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }              // si aplica
        public int Cantidad { get; set; } = 1;

        public decimal TotalLinea => Precio * Cantidad;
    }
}