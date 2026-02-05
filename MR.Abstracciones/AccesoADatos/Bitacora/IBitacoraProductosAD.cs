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

        Task RegistrarAsync(string accion, string tablaAfectada, int? idUsuario = null);
        Task<IEnumerable<BitacoraProductosDto>> ListarAsync(int top = 100);

    }
}
