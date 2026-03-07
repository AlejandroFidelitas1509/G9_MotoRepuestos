using Microsoft.EntityFrameworkCore;
using MR.Abstracciones.AccesoADatos.Calendarios;
using MR.Abstracciones.EntidadesParaUI;
using MR.AccesoDatos.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.AccesoDatos.Calendarios
{
    public class BloqueosCalendarioRepositorio : IBloqueosCalendarioRepositorio
    {

        private readonly Contexto _contexto;

        public BloqueosCalendarioRepositorio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<List<BloqueosCalendarioDto>> ObtenerBloqueosAsync()
        {
            return await _contexto.BloqueosCalendario
                .OrderByDescending(b => b.FechaInicio)
                .Select(b => new BloqueosCalendarioDto
                {
                    IdBloqueo = b.IdBloqueo,
                    FechaInicio = b.FechaInicio,
                    FechaFin = b.FechaFin,
                    Motivo = b.Motivo,
                    Activo = b.Activo
                })
                .ToListAsync();
        }

        public async Task<BloqueosCalendarioDto?> ObtenerBloqueoPorIdAsync(int id)
        {
            return await _contexto.BloqueosCalendario
                .Where(b => b.IdBloqueo == id)
                .Select(b => new BloqueosCalendarioDto
                {
                    IdBloqueo = b.IdBloqueo,
                    FechaInicio = b.FechaInicio,
                    FechaFin = b.FechaFin,
                    Motivo = b.Motivo,
                    Activo = b.Activo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> AgregarBloqueoAsync(BloqueosCalendarioDto dto)
        {
            var bloqueo = new BloqueosCalendario
            {
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                Motivo = dto.Motivo,
                Activo = dto.Activo
            };

            _contexto.BloqueosCalendario.Add(bloqueo);
            await _contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActualizarBloqueoAsync(BloqueosCalendarioDto dto)
        {
            var bloqueoExistente = await _contexto.BloqueosCalendario.FindAsync(dto.IdBloqueo);

            if (bloqueoExistente == null)
                return false;

            bloqueoExistente.FechaInicio = dto.FechaInicio;
            bloqueoExistente.FechaFin = dto.FechaFin;
            bloqueoExistente.Motivo = dto.Motivo;
            bloqueoExistente.Activo = dto.Activo;

            await _contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarBloqueoAsync(int id)
        {
            var bloqueo = await _contexto.BloqueosCalendario.FindAsync(id);

            if (bloqueo == null)
                return false;

            _contexto.BloqueosCalendario.Remove(bloqueo);
            await _contexto.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EstaFechaBloqueadaAsync(DateTime fecha)
        {
            return await _contexto.BloqueosCalendario
                .AnyAsync(b => b.Activo &&
                               b.FechaInicio <= fecha &&
                               b.FechaFin > fecha);
        }

    }
}
