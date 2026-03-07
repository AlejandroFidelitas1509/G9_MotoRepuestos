using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class BloqueosCalendarioDto
    {

        public int IdBloqueo { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime FechaFin { get; set; }

        public string? Motivo { get; set; }

        public bool Activo { get; set; } = true;

    }
}
