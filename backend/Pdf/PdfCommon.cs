using System.Globalization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MemberEligibility.Api.Dtos;

namespace MemberEligibility.Api.Pdf;

public static class PdfStyles
{
    public static IContainer HeaderStyle(IContainer container) =>
        container.Background(Colors.Grey.Lighten3).Padding(3).BorderBottom(1).BorderColor(Colors.Grey.Darken1);

    public static IContainer BodyStyle(IContainer container) =>
        container.Padding(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

    public static IContainer TotalStyle(IContainer container) =>
        container.Background(Colors.Grey.Lighten2).Padding(3).BorderTop(1).BorderColor(Colors.Grey.Darken1);

    public static string Money(decimal value) =>
        "$" + value.ToString("#,##0.00", CultureInfo.InvariantCulture);

    public static string Date(DateOnly? value) =>
        value.HasValue ? value.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) : "-";

    public static string Period(DateOnly? from, DateOnly? to) => (from, to) switch
    {
        (null, null) => "All dates",
        (not null, null) => $"From {Date(from)}",
        (null, not null) => $"Through {Date(to)}",
        _ => $"{Date(from)} – {Date(to)}",
    };
}

// Claim-line table from the CMS sample EOB, shared by the single-claim and member-level EOBs.
public static class EobLinesTable
{
    public static void Compose(IContainer container, IReadOnlyList<EobLineDto> lines, EobTotalsDto t)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(20);  // line no
                columns.ConstantColumn(55);  // date of service
                columns.RelativeColumn(2);   // description
                columns.ConstantColumn(40);  // status
                columns.ConstantColumn(50);  // provider charges
                columns.ConstantColumn(50);  // allowed charges
                columns.ConstantColumn(40);  // copay
                columns.ConstantColumn(45);  // deductible
                columns.ConstantColumn(50);  // coinsurance
                columns.ConstantColumn(50);  // paid by insurer
                columns.ConstantColumn(45);  // what you owe
                columns.ConstantColumn(35);  // remark
            });

            table.Header(header =>
            {
                void HeaderCell(string text) => header.Cell().Element(PdfStyles.HeaderStyle).Text(text).Bold().FontSize(7);
                HeaderCell("Line");
                HeaderCell("Date of Svc");
                HeaderCell("Service Description");
                HeaderCell("Status");
                HeaderCell("Provider Chg");
                HeaderCell("Allowed Chg");
                HeaderCell("CoPay");
                HeaderCell("Deductible");
                HeaderCell("Coinsurance");
                HeaderCell("Paid by Insurer");
                HeaderCell("What You Owe");
                HeaderCell("Remark");
            });

            foreach (var line in lines)
            {
                table.Cell().Element(PdfStyles.BodyStyle).Text(line.LineNo.ToString());
                table.Cell().Element(PdfStyles.BodyStyle).Text(line.DateOfService.ToString("MM/dd/yy"));
                table.Cell().Element(PdfStyles.BodyStyle).Text(line.ServiceDescription);
                table.Cell().Element(PdfStyles.BodyStyle).Text(line.ClaimStatus);
                table.Cell().Element(PdfStyles.BodyStyle).AlignRight().Text(PdfStyles.Money(line.ProviderCharges));
                table.Cell().Element(PdfStyles.BodyStyle).AlignRight().Text(PdfStyles.Money(line.AllowedCharges));
                table.Cell().Element(PdfStyles.BodyStyle).AlignRight().Text(PdfStyles.Money(line.Copay));
                table.Cell().Element(PdfStyles.BodyStyle).AlignRight().Text(PdfStyles.Money(line.Deductible));
                table.Cell().Element(PdfStyles.BodyStyle).AlignRight().Text(PdfStyles.Money(line.Coinsurance));
                table.Cell().Element(PdfStyles.BodyStyle).AlignRight().Text(PdfStyles.Money(line.PaidByInsurer));
                table.Cell().Element(PdfStyles.BodyStyle).AlignRight().Text(PdfStyles.Money(line.WhatYouOwe));
                table.Cell().Element(PdfStyles.BodyStyle).Text(line.RemarkCode ?? "");
            }

            table.Cell().ColumnSpan(4).Element(PdfStyles.TotalStyle).AlignRight().Text("Total").Bold();
            table.Cell().Element(PdfStyles.TotalStyle).AlignRight().Text(PdfStyles.Money(t.ProviderCharges)).Bold();
            table.Cell().Element(PdfStyles.TotalStyle).AlignRight().Text(PdfStyles.Money(t.AllowedCharges)).Bold();
            table.Cell().Element(PdfStyles.TotalStyle).AlignRight().Text(PdfStyles.Money(t.Copay)).Bold();
            table.Cell().Element(PdfStyles.TotalStyle).AlignRight().Text(PdfStyles.Money(t.Deductible)).Bold();
            table.Cell().Element(PdfStyles.TotalStyle).AlignRight().Text(PdfStyles.Money(t.Coinsurance)).Bold();
            table.Cell().Element(PdfStyles.TotalStyle).AlignRight().Text(PdfStyles.Money(t.PaidByInsurer)).Bold();
            table.Cell().Element(PdfStyles.TotalStyle).AlignRight().Text(PdfStyles.Money(t.WhatYouOwe)).Bold();
            table.Cell().Element(PdfStyles.TotalStyle).Text("");
        });
    }
}

// Descriptions for the remark and group/reason codes that appear in the seeded data.
// Reason codes follow the X12 Claim Adjustment Reason Code set; PDC is from the CMS sample EOB.
public static class CodeGlossary
{
    private static readonly Dictionary<string, string> Descriptions = new()
    {
        ["PDC"] = "Billed amount is higher than the maximum payment insurance allows. The payment is for the allowed amount.",
        ["CO-45"] = "Charge exceeds fee schedule/maximum allowable or contracted/legislated fee arrangement (contractual obligation).",
        ["CO-96"] = "Non-covered charge(s).",
    };

    public static List<(string Code, string Text)> For(IEnumerable<string?> codes) =>
        codes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c!)
            .Distinct()
            .OrderBy(c => c, StringComparer.Ordinal)
            .Select(c => (c, Descriptions.GetValueOrDefault(c, "See plan for details.")))
            .ToList();

    public static void Compose(IContainer container, IReadOnlyList<(string Code, string Text)> entries)
    {
        container.Column(col =>
        {
            col.Item().Text("GLOSSARY: Reason and remark codes").Bold().FontSize(8);
            foreach (var (code, text) in entries)
            {
                col.Item().PaddingTop(2).Row(row =>
                {
                    row.ConstantItem(50).Text(code).SemiBold().FontSize(7);
                    row.RelativeItem().Text(text).FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            }
        });
    }
}
