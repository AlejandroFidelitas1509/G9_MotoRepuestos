using Microsoft.AspNetCore.Mvc;
using G9MotoRepuestos.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace G9MotoRepuestos.Controllers

{
    public class ReportesController : Controller
    {
        private readonly IVentasService _ventas;

        public ReportesController()
        {
        }

        public ReportesController(IVentasService ventas)
        {
            _ventas = ventas;
        }






        [HttpGet]
        public async Task<IActionResult> VentasDiarias(DateTime? desde, DateTime? hasta)
        {
            var d = (desde ?? DateTime.Today.AddDays(-7)).Date;
            var h = (hasta ?? DateTime.Today).Date;

            var data = await _ventas.TotalesDiariosAsync(d, h);

            ViewBag.Desde = d.ToString("yyyy-MM-dd");
            ViewBag.Hasta = h.ToString("yyyy-MM-dd");


            return View(data);
        }


        [HttpGet]
        public async Task<IActionResult> VentasDiariasPdf(DateTime? desde, DateTime? hasta)
        {
            var d = (desde ?? DateTime.Today.AddDays(-7)).Date;
            var h = (hasta ?? DateTime.Today).Date;

            var data = await _ventas.TotalesDiariosAsync(d, h);

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(25);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text($"Reporte de Ventas Diarias ({d:yyyy-MM-dd} a {h:yyyy-MM-dd})")
                        .SemiBold().FontSize(16);

                    page.Content().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellHeader).Text("Fecha");
                            header.Cell().Element(CellHeader).AlignRight().Text("Total");
                        });

                        foreach (var row in data.OrderBy(x => x.Key))
                        {
                            table.Cell().Element(CellBody).Text(row.Key.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellBody).AlignRight().Text($"₡{row.Value:N2}");
                        }

                        static IContainer CellHeader(IContainer c) =>
                            c.Background(Colors.Black).Padding(6).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());

                        static IContainer CellBody(IContainer c) =>
                            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(6);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Generado: ").SemiBold();
                        x.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                    });
                });
            }).GeneratePdf();

            return File(pdf, "application/pdf", $"reporte_ventas_{d:yyyyMMdd}_{h:yyyyMMdd}.pdf");
        }
    }
}
