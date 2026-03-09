using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MR.Abstracciones.AccesoADatos.Bitacora;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;

namespace MR.LogicaNegocio.Bitacora
{
    public class BitacoraProductosLN : IBitacoraProductosLN
    {
        private readonly IBitacoraProductosAD _ad;

        public BitacoraProductosLN(IBitacoraProductosAD ad)
        {
            _ad = ad;
        }

        public Task RegistrarAsync(BitacoraProductosDto b)
            => _ad.RegistrarAsync(b);

        public Task<IEnumerable<BitacoraProductosDto>> ListarAsync(
            int top = 200,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? accion = null,
            int? idUsuario = null)
            => _ad.ListarAsync(top, desde, hasta, accion, idUsuario);

        // ✅ NUEVO: Listado paginado (para la vista con paginación)
        public Task<PagedResult<BitacoraProductosDto>> ListarPaginadoAsync(
            int page,
            int pageSize,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? accion = null,
            int? idUsuario = null)
            => _ad.ListarPaginadoAsync(page, pageSize, desde, hasta, accion, idUsuario);

        public Task<IEnumerable<string>> ListarAccionesAsync()
            => _ad.ListarAccionesAsync();

        public Task<IEnumerable<(int IdUsuario, string Nombre)>> ListarUsuariosAsync()
            => _ad.ListarUsuariosAsync();
    }
}