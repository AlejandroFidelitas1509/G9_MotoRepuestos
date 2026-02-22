using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.AccesoDatos.Entidades
{
    public class Citas
    {



        public int IdCita { get; set; }
        
        public string Detalle { get; set; }
        public DateTime Fecha { get; set; }

        public string Modelo { get; set; }

        public string Placa { get; set; }
        public int Estado { get; set; }


        public int IdUsuario { get; set; }


    }
}
