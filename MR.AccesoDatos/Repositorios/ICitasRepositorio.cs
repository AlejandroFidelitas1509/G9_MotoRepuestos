using MR.AccesoDatos.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.AccesoDatos.Repositorios
{
    public interface ICitasRepositorio
    {
        Task<List<Citas>> ObtenerCitasAsync();
        Task<Citas> ObtenerCitaPorIdAsync(int id);
        Task<bool> AgregarCitaAsync(Citas cita);
        Task<bool> ActualizarCitaAsync(Citas cita);
        Task<bool> EliminarCitaAsync(int id);


    }
}
