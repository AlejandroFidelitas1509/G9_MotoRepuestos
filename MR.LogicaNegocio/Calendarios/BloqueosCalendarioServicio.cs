using MR.Abstracciones.AccesoADatos.Calendarios;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Calendarios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.LogicaNegocio.Calendarios
{
    public class BloqueosCalendarioServicio : IBloqueosCalendarioServicio
    {

        private readonly IBloqueosCalendarioRepositorio _bloqueosCalendarioRepositorio;

        public BloqueosCalendarioServicio(IBloqueosCalendarioRepositorio bloqueosCalendarioRepositorio)
        {
            _bloqueosCalendarioRepositorio = bloqueosCalendarioRepositorio;
        }

        public async Task<CustomResponse<List<BloqueosCalendarioDto>>> ObtenerBloqueosAsync()
        {
            var respuesta = new CustomResponse<List<BloqueosCalendarioDto>>();
            respuesta.Data = await _bloqueosCalendarioRepositorio.ObtenerBloqueosAsync();
            return respuesta;
        }

        public async Task<CustomResponse<BloqueosCalendarioDto>> ObtenerBloqueoPorIdAsync(int id)
        {
            var respuesta = new CustomResponse<BloqueosCalendarioDto>();
            var bloqueo = await _bloqueosCalendarioRepositorio.ObtenerBloqueoPorIdAsync(id);

            if (bloqueo == null)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Bloqueo no encontrado.";
                return respuesta;
            }

            respuesta.Data = bloqueo;
            return respuesta;
        }

        public async Task<CustomResponse<BloqueosCalendarioDto>> AgregarBloqueoAsync(BloqueosCalendarioDto dto)
        {
            var respuesta = new CustomResponse<BloqueosCalendarioDto>();

            if (dto == null)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Datos no proporcionados.";
                return respuesta;
            }

            if (dto.FechaFin <= dto.FechaInicio)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "La fecha final debe ser mayor que la fecha inicial.";
                return respuesta;
            }

            var ok = await _bloqueosCalendarioRepositorio.AgregarBloqueoAsync(dto);

            if (!ok)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Error al agregar el bloqueo.";
                return respuesta;
            }

            return respuesta;
        }

        public async Task<CustomResponse<BloqueosCalendarioDto>> ActualizarBloqueoAsync(BloqueosCalendarioDto dto)
        {
            var respuesta = new CustomResponse<BloqueosCalendarioDto>();

            if (dto == null)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Datos no proporcionados.";
                return respuesta;
            }

            if (dto.FechaFin <= dto.FechaInicio)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "La fecha final debe ser mayor que la fecha inicial.";
                return respuesta;
            }

            var ok = await _bloqueosCalendarioRepositorio.ActualizarBloqueoAsync(dto);

            if (!ok)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Error al actualizar el bloqueo.";
                return respuesta;
            }

            return respuesta;
        }

        public async Task<CustomResponse<BloqueosCalendarioDto>> EliminarBloqueoAsync(int id)
        {
            var respuesta = new CustomResponse<BloqueosCalendarioDto>();

            var ok = await _bloqueosCalendarioRepositorio.EliminarBloqueoAsync(id);

            if (!ok)
            {
                respuesta.EsError = true;
                respuesta.Mensaje = "Error al eliminar el bloqueo.";
                return respuesta;
            }

            return respuesta;
        }

    }
}
