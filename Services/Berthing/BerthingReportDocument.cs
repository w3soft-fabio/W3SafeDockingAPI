using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebSafeDockingAPI.Models;

namespace WebSafeDockingAPI.Services
{
    public class BerthingReportDocument : IDocument
    {
        private readonly List<Berthing> _berthings;
        private readonly DateTime _dataInicial;
        private readonly DateTime _dataFinal;

        // Cores do tema marítimo
        private static readonly string NavyBlue = "#1B3A5C";
        private static readonly string LightGray = "#F2F4F7";
        private static readonly string White = "#FFFFFF";
        private static readonly string DarkText = "#1E1E1E";

        public BerthingReportDocument(List<Berthing> berthings, DateTime dataInicial, DateTime dataFinal)
        {
            _berthings = berthings;
            _dataInicial = dataInicial;
            _dataFinal = dataFinal;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.MarginVertical(30);
                page.MarginHorizontal(30);
                page.DefaultTextStyle(x => x.FontSize(8).FontColor(DarkText));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                // Barra superior azul-marinho
                column.Item().Background(NavyBlue).Padding(12).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("W3 Safe Docking")
                            .FontSize(16).Bold().FontColor(White);
                        col.Item().Text("Sistema de Monitoramento de Atracação")
                            .FontSize(9).FontColor("#B0C4DE");
                    });

                    row.ConstantItem(220).AlignRight().Column(col =>
                    {
                        col.Item().AlignRight().Text("Relatório de Atracações")
                            .FontSize(12).Bold().FontColor(White);
                        col.Item().AlignRight().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                            .FontSize(8).FontColor("#B0C4DE");
                    });
                });

                // Linha de período
                column.Item().PaddingVertical(6).Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("Período: ").Bold();
                        text.Span($"{_dataInicial:dd/MM/yyyy} a {_dataFinal:dd/MM/yyyy}");
                    });

                    row.ConstantItem(200).AlignRight().Text(text =>
                    {
                        text.Span("Total de registros: ").Bold();
                        text.Span($"{_berthings.Count}");
                    });
                });

                column.Item().PaddingBottom(4).LineHorizontal(1).LineColor("#CBD5E1");
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingTop(6).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);   // Berço
                    columns.RelativeColumn(2.5f); // Navio
                    columns.ConstantColumn(55);   // IMO
                    columns.RelativeColumn(2);    // Empresa de Amarração
                    columns.RelativeColumn(2);    // Agência
                    columns.ConstantColumn(60);   // Lado
                    columns.ConstantColumn(72);   // Chegada
                    columns.ConstantColumn(72);   // Saída
                    columns.ConstantColumn(45);   // Cal. Cheg. Proa
                    columns.ConstantColumn(45);   // Cal. Cheg. Popa
                    columns.ConstantColumn(45);   // Cal. Saída Proa
                    columns.ConstantColumn(45);   // Cal. Saída Popa
                });

                // Cabeçalho da tabela
                table.Header(header =>
                {
                    var headers = new[]
                    {
                        "Berço", "Navio", "IMO", "Empresa Amarração", "Agência",
                        "Lado", "Chegada", "Saída",
                        "Cal.Ch.Pr", "Cal.Ch.Pp", "Cal.Sa.Pr", "Cal.Sa.Pp"
                    };

                    foreach (var h in headers)
                    {
                        header.Cell().Background(NavyBlue).Padding(4)
                            .Text(h).FontSize(7).Bold().FontColor(White);
                    }
                });

                // Linhas de dados com cores alternadas
                for (int i = 0; i < _berthings.Count; i++)
                {
                    var b = _berthings[i];
                    var bgColor = i % 2 == 0 ? White : LightGray;

                    TableCell(table, bgColor, b.Berth.ToString());
                    TableCell(table, bgColor, b.Ship?.Name ?? "—");
                    TableCell(table, bgColor, b.Ship?.Imo.ToString() ?? "—");
                    TableCell(table, bgColor, b.MooringCompany?.Name ?? "—");
                    TableCell(table, bgColor, b.ShippingAgency?.Name ?? "—");
                    TableCell(table, bgColor, b.Side ?? "—");
                    TableCell(table, bgColor, b.ArrivalAt?.ToString("dd/MM/yy HH:mm") ?? "—");
                    TableCell(table, bgColor, b.DepartureAt?.ToString("dd/MM/yy HH:mm") ?? "—");
                    TableCell(table, bgColor, FormatDecimal(b.ArrivalDraftFore));
                    TableCell(table, bgColor, FormatDecimal(b.ArrivalDraftAft));
                    TableCell(table, bgColor, FormatDecimal(b.DepartureDraftFore));
                    TableCell(table, bgColor, FormatDecimal(b.DepartureDraftAft));
                }
            });
        }

        private static void TableCell(TableDescriptor table, string bgColor, string text)
        {
            table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor("#E2E8F0")
                .Padding(3).Text(text).FontSize(7);
        }

        private static string FormatDecimal(decimal? value) =>
            value.HasValue ? $"{value.Value:N2}" : "—";

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(0.5f).LineColor("#CBD5E1");
                col.Item().PaddingTop(4).Row(row =>
                {
                    row.RelativeItem().Text("Gerado por W3 Safe Docking")
                        .FontSize(7).FontColor("#64748B");

                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.CurrentPageNumber().FontSize(7);
                        text.Span(" / ").FontSize(7);
                        text.TotalPages().FontSize(7);
                    });
                });
            });
        }
    }
}
