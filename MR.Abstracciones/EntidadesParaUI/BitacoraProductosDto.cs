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
        public DateTime Fecha { get; set; }
        public string Accion { get; set; } = "";
        public string TablaAfectada { get; set; } = "";

        public int? IdUsuario { get; set; }
        public string? UsuarioNombre { get; set; }

        public int? RegistroId { get; set; }
        public string? Descripcion { get; set; }

        public string? AntesJson { get; set; }
        public string? DespuesJson { get; set; }

        public string? NombreProducto { get; set; }

    }
}
