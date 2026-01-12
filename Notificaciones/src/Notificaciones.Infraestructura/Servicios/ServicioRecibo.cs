using Notificaciones.Aplicacion.Interfaces;
using Pagos.Aplicacion.Eventos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace Notificaciones.Infraestructura.Servicios;

public class ServicioRecibo : IServicioRecibo
{
    public byte[] GenerarPdfRecibo(PagoAprobadoEvento evento)
    {
        // QuestPDF requiere configurar la licencia (Community es gratis para proyectos pequeños/educativos)
        QuestPDF.Settings.License = LicenseType.Community;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily("DejaVu Sans"));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("KAIRO EVENTOS").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("Comprobante de Pago Electrónico").FontSize(10).Italic();
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Recibo #: {evento.TransaccionId.ToString().Substring(0, 8).ToUpper()}");
                        col.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}");
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Spacing(20);

                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(100);
                            columns.RelativeColumn();
                        });

                        table.Cell().Text("Usuario ID:").SemiBold();
                        table.Cell().Text(evento.UsuarioId);

                        table.Cell().Text("Orden ID:").SemiBold();
                        table.Cell().Text(evento.OrdenId);
                    });

                    x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Descripción").SemiBold();
                            header.Cell().AlignRight().Text("Monto").SemiBold();
                        });

                        table.Cell().PaddingVertical(5).Text($"Acceso a Evento - Orden {evento.OrdenId}");
                        table.Cell().PaddingVertical(5).AlignRight().Text($"${evento.Monto:N2}");
                    });

                    x.Item().AlignRight().Text(text =>
                    {
                        text.Span("TOTAL: ").FontSize(16).SemiBold();
                        text.Span($"${evento.Monto:N2}").FontSize(16).SemiBold().FontColor(Colors.Green.Medium);
                    });

                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Gracias por confiar en ");
                    x.Span("Kairo Eventos").SemiBold();
                });
            });
        });

        using var stream = new MemoryStream();
        documento.GeneratePdf(stream);
        return stream.ToArray();
    }
}
