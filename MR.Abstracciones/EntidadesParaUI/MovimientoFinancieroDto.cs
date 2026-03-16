using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class MovimientoFinancieroDto
    {

        public int IdFinanzas { get; set; }

        public string Tipo { get; set; } = string.Empty;
  

        public decimal Monto { get; set; }

        public DateTime Fecha { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public int IdUsuario { get; set; }

        public string? UsuarioNombre { get; set; }

        public string? Origen { get; set; }
        

        public int? IdReferencia { get; set; }
       

    }
}
