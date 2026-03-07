using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.AccesoDatos.Entidades
{

    [Table("BloqueosCalendario")]
    public class BloqueosCalendario
    {

        [Key]
        public int IdBloqueo { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaFin { get; set; }

        public string? Motivo { get; set; }

        public bool Activo { get; set; }
    }

}

