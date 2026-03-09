using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.Abstracciones.AccesoADatos.Bitacora
{
    public interface IBitacoraProductosAD
    {
        Task RegistrarAsync(BitacoraProductosDto b);

        Task<IEnumerable<BitacoraProductosDto>> ListarAsync(
            int top = 200,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? accion = null,
            int? idUsuario = null
        );

        Task<PagedResult<BitacoraProductosDto>> ListarPaginadoAsync(
            int page,
            int pageSize,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? accion = null,
            int? idUsuario = null
        );

        Task<IEnumerable<string>> ListarAccionesAsync();

        Task<IEnumerable<(int IdUsuario, string Nombre)>> ListarUsuariosAsync();
    }
}
