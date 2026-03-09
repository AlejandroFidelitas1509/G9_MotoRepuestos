using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.EntidadesParaUI
{
    public class CustomResponse <T>
    {

        public bool EsError { get; set; } = false;
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }

    }
}
