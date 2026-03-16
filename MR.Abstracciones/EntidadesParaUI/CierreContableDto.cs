using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class CierreContableDto
    {

        public int IdCierre { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public DateTime FechaRegistro { get; set; }

        public string Tipo { get; set; } = string.Empty;

        public decimal TotalIngresos { get; set; }

        public decimal TotalEgresos { get; set; }

        public decimal BalanceTotal { get; set; }

        public int IdUsuario { get; set; }

        public string? UsuarioNombre { get; set; }

    }
}
