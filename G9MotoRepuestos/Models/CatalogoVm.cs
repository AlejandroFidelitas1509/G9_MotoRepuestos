using MR.Abstracciones.EntidadesParaUI;

namespace G9MotoRepuestos.Models
{
    public class CatalogoVm
    {

        public IEnumerable<ProductoDto> Productos { get; set; } = Enumerable.Empty<ProductoDto>();
        public IEnumerable<CategoriaDto> Categorias { get; set; } = Enumerable.Empty<CategoriaDto>();

        public int? CategoriaId { get; set; }

    }
}
