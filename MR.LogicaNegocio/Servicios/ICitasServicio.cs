using MR.LogicaNegocio.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.LogicaNegocio.Servicios
{
    public interface ICitasServicio
    {
        Task <CustomResponse<CitasDto>> ObtenerCitaPorIdAsync(int id);

        Task <CustomResponse<List<CitasDto>>> ObtenerCitasAsync();

        Task <CustomResponse<CitasDto>> AgregarCitaAsync(CitasDto citaDto);

        Task <CustomResponse<CitasDto>> ActualizarCitaAsync(CitasDto citaDto);

        Task <CustomResponse<CitasDto>> EliminarCitaAsync(int id);

    }
}
