using Microsoft.EntityFrameworkCore;
using MR.AccesoDatos.Entidades;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MR.AccesoDatos.Repositorios
{
    public class CitasRepositorio : ICitasRepositorio
    {
        private readonly Contexto _contexto;

        public CitasRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<bool> AgregarCitaAsync(Citas cita)
        {
            _contexto.Citas.Add(cita); // Inserta en la tabla
            await _contexto.SaveChangesAsync(); // Guarda cambios en la BD
            return true;
        }

        public async Task<bool> ActualizarCitaAsync(Citas cita)
        {
            _contexto.Citas.Update(cita);
            await _contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarCitaAsync(int id)
        {
            var cita = await _contexto.Citas.FindAsync(id);
            if (cita != null)
            {
                _contexto.Citas.Remove(cita);
                await _contexto.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<Citas> ObtenerCitaPorIdAsync(int id)
        {
            return await _contexto.Citas.FindAsync(id);
        }

        public async Task<List<Citas>> ObtenerCitasAsync()
        {
            return await _contexto.Citas.ToListAsync();
        }
    }
}