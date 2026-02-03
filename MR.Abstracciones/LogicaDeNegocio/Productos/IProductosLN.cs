using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MR.Abstracciones.EntidadesParaUI;

namespace MR.Abstracciones.LogicaDeNegocio.Productos
{
    public interface IProductosLN
    {

        Task<IEnumerable<ProductoDto>> ListarAsync(bool soloActivos = true);
        Task<ProductoDto?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(ProductoDto producto);
        Task<bool> ActualizarAsync(ProductoDto producto);
        Task<bool> ActivarAsync(int id);
        Task<bool> DesactivarAsync(int id);

    }
}
