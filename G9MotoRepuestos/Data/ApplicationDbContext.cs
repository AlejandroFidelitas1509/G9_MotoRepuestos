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

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }

        // 
        public DbSet<Finanzas> Finanzas { get; set; }
        public DbSet<Cierres> Cierres { get; set; }
    }
}
