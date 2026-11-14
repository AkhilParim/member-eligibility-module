using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MemberEligibility.Api.Dtos;
using static MemberEligibility.Api.Pdf.PdfStyles;

namespace MemberEligibility.Api.Pdf;

// Flat PMPM capitation statement for one provider contract and payment month.
public static class CapitationPdfGenerator
{
    public static byte[] Generate(CapitationDetailDto detail)
    {
        var contract = detail.Contract;
        var totals = detail.Totals;
        var month = detail.PaymentMonth.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(2).Column(c =>
                        {
                            c.Item().Text("CAPITATION STATEMENT").Bold().FontSize(16);
                            c.Item().Text($"Flat PMPM capitation for {month}").FontColor(Colors.Grey.Darken2);
                        });
                        row.RelativeItem(1).Column(c =>
                        {
                            c.Item().Text(contract.ProviderGroupName).Bold();
                            c.Item().Text($"Contract: {contract.ContractNumber}");
                            c.Item().Text($"TIN: {contract.ProviderTin ?? "-"}");
                        });
                    });

                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text($"Payment Month: {month}");
                        row.RelativeItem().Text($"PMPM Rate: {Money(contract.PmpmRate)}");
                        row.RelativeItem().Text($"Effective: {Date(contract.EffectiveDate)} –{(contract.TermDate.HasValue ? Date(contract.TermDate) : "open")}");
                        row.RelativeItem().Text($"Members: {totals.MemberCount}");
                        row.RelativeItem().Text($"Statement Date: {Date(DateOnly.FromDateTime(DateTime.UtcNow))}");
                    });

                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(10);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);   // member
                            columns.ConstantColumn(85);  // MBI
                            columns.ConstantColumn(55);  // risk factor
                            columns.ConstantColumn(55);  // regular months
                            columns.ConstantColumn(60);  // adjustment months
                            columns.ConstantColumn(65);  // member-months
                            columns.ConstantColumn(65);  // PMPM rate
                            columns.ConstantColumn(85);  // MMR premium (reference)
                            columns.ConstantColumn(85);  // capitation
                        });

                        table.Header(header =>
                        {
                            void H(string text, bool right = true)
                            {
                                var cell = header.Cell().Element(HeaderStyle);
                                if (right) cell.AlignRight().Text(text).Bold().FontSize(7);
                                else cell.Text(text).Bold().FontSize(7);
                            }

                            H("Member", false);
                            H("MBI", false);
                            H("Risk Factor");
                            H("Regular Months");
                            H("Adjustment Months");
                            H("Member-Months");
                            H("PMPM Rate");
                            H("MMR Premium (ref.)");
                            H("Capitation");
                        });

                        foreach (var m in detail.Members)
                        {
                            table.Cell().Element(BodyStyle).Text(m.MemberName);
                            table.Cell().Element(BodyStyle).Text(m.Mbi ?? "-");
                            table.Cell().Element(BodyStyle).AlignRight().Text(m.RiskFactor?.ToString("0.0000", CultureInfo.InvariantCulture) ?? "-");
                            table.Cell().Element(BodyStyle).AlignRight().Text(m.RegularMonths.ToString());
                            table.Cell().Element(BodyStyle).AlignRight().Text(m.AdjustmentMonths.ToString("+0;-0;0"));
                            table.Cell().Element(BodyStyle).AlignRight().Text(m.MemberMonths.ToString());
                            table.Cell().Element(BodyStyle).AlignRight().Text(Money(contract.PmpmRate));
                            table.Cell().Element(BodyStyle).AlignRight().Text(Money(m.MmrPremium));
                            table.Cell().Element(BodyStyle).AlignRight().Text(Money(m.Capitation)).SemiBold();
                        }

                        table.Cell().ColumnSpan(5).Element(TotalStyle).AlignRight().Text($"Contract total ({totals.MemberCount} members)").Bold();
                        table.Cell().Element(TotalStyle).AlignRight().Text(totals.MemberMonths.ToString()).Bold();
                        table.Cell().Element(TotalStyle).Text("");
                        table.Cell().Element(TotalStyle).AlignRight().Text(Money(totals.MmrPremium)).Bold();
                        table.Cell().Element(TotalStyle).AlignRight().Text(Money(totals.Capitation)).Bold();
                    });

                    col.Item().Text(text =>
                    {
                        text.DefaultTextStyle(s => s.FontSize(8).FontColor(Colors.Grey.Darken2));
                        text.Line("How this is calculated").Bold();
                        text.Line("Capitation = PMPM rate x member-months. A regular MMR row counts +1 member-month; a retroactive adjustment row counts -1 (reversal) or +1 (late add), so a negative line is a clawback.");
                        text.Line("MMR premium (Part A + Part B) is shown for reference only and is not used in the calculation."
                            + (totals.CapitationPercentOfPremium is { } pct ? $" Total capitation is {pct:0.0}% of the MMR premium for these members." : ""));
                    });
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text($"{contract.ProviderGroupName} · {contract.ContractNumber} · {month}")
                            .FontSize(7).FontColor(Colors.Grey.Darken2);
                        row.ConstantItem(80).AlignRight().Text(t =>
                        {
                            t.DefaultTextStyle(s => s.FontSize(7).FontColor(Colors.Grey.Darken2));
                            t.Span("Page ");
                            t.CurrentPageNumber();
                            t.Span(" of ");
                            t.TotalPages();
                        });
                    });
                });
            });
        }).GeneratePdf();
    }
}
