using MR.Abstracciones.EntidadesParaUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.LogicaDeNegocio.Finanzas
{
    public interface IFinanzasLN
    {

        Task<IEnumerable<MovimientoFinancieroDto>> ObtenerMovimientosAsync(
            string? textoBusqueda,
            DateTime? desde,
            DateTime? hasta,
            string? tipoFiltro,
            string? origenFiltro
        );

        Task<ResumenContabilidadDto> ObtenerResumenAsync(
            DateTime? desde,
            DateTime? hasta,
            string? tipoFiltro,
            string? origenFiltro
        );

        Task<int> RegistrarMovimientoAsync(MovimientoFinancieroDto movimiento);

        Task<IEnumerable<CierreContableDto>> ObtenerCierresAsync();

        Task<int> RegistrarCierreAsync(CierreContableDto cierre);

        Task<int> RegistrarIngresoAutomaticoAsync(
            decimal monto,
            int idUsuario,
            string categoria,
            string descripcion,
            string origen,
            int? idReferencia
        );

        Task<int> RegistrarEgresoAutomaticoAsync(
            decimal monto,
            int idUsuario,
            string categoria,
            string descripcion,
            string origen,
            int? idReferencia
        );

    }
}
