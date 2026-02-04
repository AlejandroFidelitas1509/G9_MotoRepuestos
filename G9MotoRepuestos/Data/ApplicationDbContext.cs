using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace G9MotoRepuestos.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Cambiamos a 'Usuario' (singular) que es como se llama tu clase en Models
        public DbSet<G9MotoRepuestos.Models.Usuario> Usuarios { get; set; }

        // Usamos 'new' para evitar el choque con Identity y 'Rol' en singular
        public new DbSet<G9MotoRepuestos.Models.Rol> Roles { get; set; }
    }
}
