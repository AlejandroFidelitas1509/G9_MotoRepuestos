namespace G9MotoRepuestos.Models
{
    public class Usuarios
    {
        public int IdUsuario { get; set; }  
        public string NombreCompleto { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; }
        public bool Estado { get; set; }
        public string ImagenURL { get; set; }
    }
}
