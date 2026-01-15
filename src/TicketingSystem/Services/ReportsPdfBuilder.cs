using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TicketingSystem.ViewModels;

namespace TicketingSystem.Services;

public class ReportsPdfBuilder
{
    public byte[] Build(ReportsViewModel model)
    {
        var generatedLocal = model.GeneratedAtUtc.ToLocalTime();
        var totalSla = Math.Max(model.OnTrackCount + model.DueSoonCount + model.OverdueCount, 1);
        var maxWorkload = Math.Max(model.WorkloadItems.Any() ? model.WorkloadItems.Max(i => i.Count) : 0, 1);
        const float barMaxWidth = 220f;

        var volumePoints = model.VolumeLabels
            .Select((label, index) => new
            {
                Label = label,
                Created = model.VolumeCreatedCounts.ElementAtOrDefault(index),
                Closed = model.VolumeClosedCounts.ElementAtOrDefault(index)
            })
            .TakeLast(14)
            .ToList();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(text => text.FontSize(11));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Ticketing System Report").FontSize(18).SemiBold();
                        col.Item().Text($"Generated {generatedLocal:MMM d, yyyy h:mm tt}").FontSize(10).FontColor("#6b7280");
                    });
                    row.ConstantItem(120).AlignRight().Text($"Range: {model.StartDateUtc:MMM d} - {model.EndDateUtc:MMM d}");
                });

                page.Content().Column(col =>
                {
                    col.Spacing(14);

                    col.Item().Text("Summary").FontSize(14).SemiBold();
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor("#d9dee6").Padding(8).Column(card =>
                        {
                            card.Item().Text("Open Tickets").FontSize(10).FontColor("#6b7280");
                            card.Item().Text(model.TotalOpenCount.ToString()).FontSize(18).SemiBold();
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor("#d9dee6").Padding(8).Column(card =>
                        {
                            card.Item().Text("Closed (30d)").FontSize(10).FontColor("#6b7280");
                            card.Item().Text(model.ClosedLast30DaysCount.ToString()).FontSize(18).SemiBold();
                        });
                        row.Spacing(10);
                        row.RelativeItem().Border(1).BorderColor("#d9dee6").Padding(8).Column(card =>
                        {
                            card.Item().Text("Overdue").FontSize(10).FontColor("#6b7280");
                            card.Item().Text(model.OverdueCount.ToString()).FontSize(18).SemiBold();
                        });
                    });

                    col.Item().Text("SLA Health").FontSize(13).SemiBold();
                    col.Item().Row(row =>
                    {
                        row.RelativeItem((float)Math.Max(model.OnTrackCount, 0.1))
                            .Height(10)
                            .Background("#16a34a");
                        row.RelativeItem((float)Math.Max(model.DueSoonCount, 0.1))
                            .Height(10)
                            .Background("#f59e0b");
                        row.RelativeItem((float)Math.Max(model.OverdueCount, 0.1))
                            .Height(10)
                            .Background("#dc2626");
                    });
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"On Track: {model.OnTrackCount} ({(model.OnTrackCount * 100 / totalSla)}%)").FontSize(10);
                        row.RelativeItem().Text($"Due Soon: {model.DueSoonCount} ({(model.DueSoonCount * 100 / totalSla)}%)").FontSize(10);
                        row.RelativeItem().Text($"Overdue: {model.OverdueCount} ({(model.OverdueCount * 100 / totalSla)}%)").FontSize(10);
                    });

                    col.Item().Text("Workload by Admin").FontSize(13).SemiBold();
                    col.Item().Column(list =>
                    {
                        list.Spacing(4);
                        foreach (var item in model.WorkloadItems)
                        {
                            var barWidth = barMaxWidth * item.Count / maxWorkload;
                            list.Item().Row(row =>
                            {
                                row.ConstantItem(150).Text(item.Name).FontSize(10);
                                row.ConstantItem(barWidth).Height(8).Background("#1f4d6b");
                                row.ConstantItem(barMaxWidth - barWidth).Height(8).Background("#e5e7eb");
                                row.ConstantItem(30).AlignRight().Text(item.Count.ToString()).FontSize(10);
                            });
                        }
                    });

                    col.Item().Text("Ticket Volume (Last 14 Days)").FontSize(13).SemiBold();
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Date").SemiBold();
                            header.Cell().Text("Created").SemiBold();
                            header.Cell().Text("Closed").SemiBold();
                        });

                        foreach (var point in volumePoints)
                        {
                            table.Cell().Text(point.Label);
                            table.Cell().Text(point.Created.ToString());
                            table.Cell().Text(point.Closed.ToString());
                        }
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}
