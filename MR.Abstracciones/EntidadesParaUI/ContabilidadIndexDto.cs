using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class ContabilidadIndexDto
    {

        public string? TextoBusqueda { get; set; }

        public DateTime? Desde { get; set; }

        public DateTime? Hasta { get; set; }

        public string? TipoFiltro { get; set; }
        // Ingreso / Egreso

        public string? OrigenFiltro { get; set; }
        // Venta / Compra / Servicio / Manual / Ajuste

        public ResumenContabilidadDto Resumen { get; set; } = new ResumenContabilidadDto();

        public List<MovimientoFinancieroDto> Movimientos { get; set; } = new List<MovimientoFinancieroDto>();
    }

}
