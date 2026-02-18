using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.LogicaNegocio.Dtos
{
    public class CitasDto
    {
        public int IdCita { get; set; }
        [Required(ErrorMessage ="Detalle obligatorio. ")]

        public string Detalle { get; set; }
        [Required(ErrorMessage = "Fecha obligatorio. ")]
        public DateTime Fecha { get; set; }

        public string Modelo { get; set; }


        public string Placa { get; set; }
        public int Estado { get; set; }


        public int IdUsuario { get; set; }


    }
}
