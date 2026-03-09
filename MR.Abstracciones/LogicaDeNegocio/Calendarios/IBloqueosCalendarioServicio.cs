using MR.Abstracciones.EntidadesParaUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MR.Abstracciones.LogicaDeNegocio.Calendarios
{
    public interface IBloqueosCalendarioServicio
    {

        Task<CustomResponse<List<BloqueosCalendarioDto>>> ObtenerBloqueosAsync();
        Task<CustomResponse<BloqueosCalendarioDto>> ObtenerBloqueoPorIdAsync(int id);
        Task<CustomResponse<BloqueosCalendarioDto>> AgregarBloqueoAsync(BloqueosCalendarioDto dto);
        Task<CustomResponse<BloqueosCalendarioDto>> ActualizarBloqueoAsync(BloqueosCalendarioDto dto);
        Task<CustomResponse<BloqueosCalendarioDto>> EliminarBloqueoAsync(int id);

    }
}
