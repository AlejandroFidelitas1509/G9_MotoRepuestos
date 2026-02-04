using Microsoft.EntityFrameworkCore;
using G9MotoRepuestos.Models;

namespace G9MotoRepuestos.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<G9MotoRepuestos.Models.Usuario> Usuarios { get; set; }

        public DbSet<G9MotoRepuestos.Models.Rol> Roles { get; set; }
    }
} 
