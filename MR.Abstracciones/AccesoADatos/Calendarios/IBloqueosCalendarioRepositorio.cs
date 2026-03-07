using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.Abstracciones.AccesoADatos.Calendarios
{
    public interface IBloqueosCalendarioRepositorio
    {
        Task<List<BloqueosCalendarioDto>> ObtenerBloqueosAsync();
        Task<BloqueosCalendarioDto?> ObtenerBloqueoPorIdAsync(int id);
        Task<bool> AgregarBloqueoAsync(BloqueosCalendarioDto dto);
        Task<bool> ActualizarBloqueoAsync(BloqueosCalendarioDto dto);
        Task<bool> EliminarBloqueoAsync(int id);
        Task<bool> EstaFechaBloqueadaAsync(DateTime fecha);

    }
}
