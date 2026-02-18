using MR.AccesoDatos.Entidades;
using MR.LogicaNegocio.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace MR.LogicaNegocio.Mapeos
{
    public class MapeoClases : Profile
    {

        public MapeoClases()
        {
            CreateMap<Citas, CitasDto>();
            CreateMap<CitasDto, Citas>();
        }




    }
}
