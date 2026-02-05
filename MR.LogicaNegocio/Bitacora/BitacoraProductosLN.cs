using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.AccesoADatos.Bitacora;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Bitacora;

namespace MR.LogicaNegocio.Bitacora
{
    public class BitacoraProductosLN : IBitacoraProductosLN
    {
        private readonly IBitacoraProductosAD _ad;
        public BitacoraProductosLN(IBitacoraProductosAD ad) => _ad = ad;

        public Task RegistrarAsync(string accion, string tablaAfectada, int? idUsuario = null)
        {
            if (string.IsNullOrWhiteSpace(accion)) throw new ArgumentException("Acción inválida.");
            if (string.IsNullOrWhiteSpace(tablaAfectada)) throw new ArgumentException("Tabla inválida.");

            return _ad.RegistrarAsync(accion.Trim(), tablaAfectada.Trim(), idUsuario);
        }

        public Task<IEnumerable<BitacoraProductosDto>> ListarAsync(int top = 100)
            => _ad.ListarAsync(top);
    }
}
