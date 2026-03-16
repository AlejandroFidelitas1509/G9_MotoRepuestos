using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class ResumenContabilidadDto
    {

        public decimal TotalIngresos { get; set; }

        public decimal TotalEgresos { get; set; }

        public decimal BalanceTotal => TotalIngresos - TotalEgresos;


    }
}
