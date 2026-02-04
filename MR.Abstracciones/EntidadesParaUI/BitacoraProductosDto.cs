using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class BitacoraProductosDto
    {

        public int IdBitacora { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Accion { get; set; }
        public string? TablaAfectada { get; set; }
        public int? IdUsuario { get; set; } // por ahora null

    }
}
