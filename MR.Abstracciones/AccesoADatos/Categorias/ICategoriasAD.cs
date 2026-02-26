using MR.Abstracciones.EntidadesParaUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.Abstracciones.AccesoADatos.Categorias
{
    public interface ICategoriasAD
    {
        Task<IEnumerable<CategoriaDto>> ListarAsync(bool soloActivas = true);
        Task<int> CrearAsync(CategoriaDto categoria);
        Task<bool> ActualizarAsync(CategoriaDto c);

        Task<bool> CambiarEstadoAsync(int idCategoria, bool estado);
    }
}
