using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace G9MotoRepuestos.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IVentasService _ventas;

        public ReportesController(IVentasService ventas)
        {
            _ventas = ventas;
        }

        public IActionResult Index() => View();

        public IActionResult RepInventario() => View();
        public IActionResult RepUsuarios() => View();
        public IActionResult RepServicios() => View();
        public IActionResult RepCitas() => View();

        // ✅ Issue 138/152: reporte ventas (vista)
        [HttpGet]
        public async Task<IActionResult> RepVentas(DateTime? desde, DateTime? hasta)
        {
            var d = (desde ?? DateTime.Today.AddDays(-7)).Date;
            var h = (hasta ?? DateTime.Today).Date;

            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");

            var data = await _ventas.TotalesDiariosAsync(d, h);
            return View(data);
        }

        // ✅ Issue 152: exportar PDF
        [HttpGet]
        public async Task<IActionResult> RepVentasPdf(DateTime? desde, DateTime? hasta)
        {
            var d = (desde ?? DateTime.Today.AddDays(-7)).Date;
            var h = (hasta ?? DateTime.Today).Date;

            var data = await _ventas.TotalesDiariosAsync(d, h);

            if (data.Count == 0)
            {
                TempData["Warning"] = "No existen registros";
                return RedirectToAction(nameof(RepVentas), new { desde = d, hasta = h });
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Reporte de Ventas Diarias ({d:yyyy-MM-dd} a {h:yyyy-MM-dd})").Bold().FontSize(16);
                        col.Item().PaddingTop(10);

                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            t.Header(header =>
                            {
                                header.Cell().Text("Día").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            foreach (var row in data)
                            {
                                t.Cell().Text(row.Key.ToString("yyyy-MM-dd"));
                                t.Cell().Text("₡" + row.Value.ToString("N2"));
                            }
                        });
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"ReporteVentas_{d:yyyyMMdd}_{h:yyyyMMdd}.pdf");
        }
    }
}
