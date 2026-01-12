using Pagos.Dominio.Entidades;
using Pagos.Dominio.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Pagos.Infraestructura.Facturacion;

public class GeneradorFacturaQuestPdf : IGeneradorFactura
{
    public byte[] GenerarPdf(Transaccion tx)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Text("FACTURA DE PAGO - KAIRO EVENTOS")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Spacing(5);
                    x.Item().Text($"Transacción ID: {tx.Id}");
                    x.Item().Text($"Orden ID: {tx.OrdenId}");
                    x.Item().Text($"Fecha: {tx.FechaCreacion:dd/MM/yyyy HH:mm}");
                    x.Item().Text($"Monto Total: {tx.Monto:C}").FontSize(14).SemiBold();
                    x.Item().Text($"Método: Tarjeta {tx.TarjetaMascara}");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }
}
