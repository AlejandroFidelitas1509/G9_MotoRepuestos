using MR.Abstracciones.AccesoADatos.Categorias;
using MR.Abstracciones.EntidadesParaUI;
using MR.Abstracciones.LogicaDeNegocio.Categorias;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MR.LogicaNegocio.Categorias
{
    public class CategoriasLN : ICategoriasLN
    {
        private readonly ICategoriasAD _ad;
        public CategoriasLN(ICategoriasAD ad) => _ad = ad;

        public Task<IEnumerable<CategoriaDto>> ListarAsync(bool soloActivas = true)
            => _ad.ListarAsync(soloActivas);

        public async Task<int> CrearAsync(CategoriaDto categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
                throw new ArgumentException("Nombre es obligatorio.");

            categoria.Nombre = categoria.Nombre.Trim();
            return await _ad.CrearAsync(categoria);
        }

        public Task<bool> ActivarAsync(int idCategoria)
            => _ad.CambiarEstadoAsync(idCategoria, true);

        public Task<bool> DesactivarAsync(int idCategoria)
            => _ad.CambiarEstadoAsync(idCategoria, false);
    }
}
