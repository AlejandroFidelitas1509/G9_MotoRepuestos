using MR.Abstracciones.EntidadesParaUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.AccesoADatos.Finanzas
{
    public interface IFinanzasAD
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

    }
}
