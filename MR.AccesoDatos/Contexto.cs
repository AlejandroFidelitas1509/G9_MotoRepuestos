using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MR.AccesoDatos.Entidades;

namespace MR.AccesoDatos
{
    public class Contexto : DbContext
    {
        public Contexto(DbContextOptions<Contexto> options)
            : base(options)
        {
        }

        public DbSet<Citas> Citas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración adicional de la entidad Citas
            modelBuilder.Entity<Citas>(entity =>
            {
                entity.HasKey(e => e.IdCita); // Clave primaria
                entity.Property(e => e.Detalle).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Modelo).HasMaxLength(100);
                entity.Property(e => e.Placa).HasMaxLength(20);
            });
        }



    }
}
