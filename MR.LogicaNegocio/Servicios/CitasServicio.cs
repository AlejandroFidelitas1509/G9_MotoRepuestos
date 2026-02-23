using AutoMapper;
using MR.AccesoDatos.Entidades;
using MR.AccesoDatos.Repositorios;
using MR.LogicaNegocio.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.LogicaNegocio.Servicios
{
    public class CitasServicio : ICitasServicio
    {
        private readonly ICitasRepositorio _citasRepositorio;
        private readonly IMapper _mapper;


        public CitasServicio(ICitasRepositorio citasRepositorio, IMapper mapper)
        {
            _citasRepositorio = citasRepositorio;
            _mapper = mapper;
        }



        public async Task<CustomResponse<CitasDto>> AgregarCitaAsync(CitasDto citaDto)
        {
            var respuesta = new CustomResponse<CitasDto>();

            if (citaDto == null)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Datos de la cita no proporcionados.";
                return respuesta;
            }

            if (citaDto.Fecha < DateTime.Now)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "La fecha y hora de la cita no pueden ser en el pasado.";
                return respuesta;
            }

            if(!await _citasRepositorio.AgregarCitaAsync(_mapper.Map<Citas>(citaDto)))
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Error al agregar la cita.";
                return respuesta;
            }
            return respuesta;


        }


        public async Task<CustomResponse<CitasDto>> ObtenerCitaPorIdAsync(int id)
        {
            var respuesta = new CustomResponse<CitasDto>();

            var cita= await _citasRepositorio.ObtenerCitaPorIdAsync(id);
            if (cita == null)
            { 
                respuesta.EsError = true;
                respuesta.Mensaje = "Cita no encontrada.";
                return respuesta;


            }
            respuesta.Data = _mapper.Map<CitasDto>(cita);
            return respuesta;

        }

        public async Task<CustomResponse<List<CitasDto>>> ObtenerCitasAsync()
        {
            var respuesta = new CustomResponse<List<CitasDto>>();
            var citas = await _citasRepositorio.ObtenerCitasAsync();
            var citasDto = _mapper.Map<List<CitasDto>>(citas);

            respuesta.Data = citasDto;
            return respuesta;
        }

        public async Task<CustomResponse<CitasDto>> ActualizarCitaAsync(CitasDto citaDto)
        {
            var respuesta = new CustomResponse<CitasDto>();

            if (citaDto == null)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Datos de la cita no proporcionados.";
                return respuesta;
            }
            return respuesta;
        }



        public async Task<CustomResponse<CitasDto>> EliminarCitaAsync(int id)
        {
            var respuesta = new CustomResponse<CitasDto>();

            if (!await _citasRepositorio.EliminarCitaAsync(id)) { 
            
                respuesta.EsError = true;
                respuesta.Mensaje = "Error al eliminar la cita.";
                return respuesta;

            }
            return respuesta;


        }





    }
}
