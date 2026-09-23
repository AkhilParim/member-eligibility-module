using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MemberEligibility.Api.Dtos;

namespace MemberEligibility.Api.Pdf;

// Layout follows the CMS sample EOB ("Reading Your Explanation of Benefits", Pub #11819, go.cms.gov/c2c).
public static class EobPdfGenerator
{
    public static byte[] Generate(EobDto eob)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                page.Header().Column(col =>
                {
                    col.Item().Text("EXPLANATION OF BENEFITS").Bold().FontSize(16);
                    col.Item().Text("THIS IS NOT A BILL").Bold().FontColor(Colors.Red.Medium);
                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Statement Date: {eob.StatementDate:MM/dd/yyyy}");
                            c.Item().Text($"Document Number: {eob.DocumentNumber}");
                            c.Item().Text($"Subscriber Number: {eob.SubscriberNumber}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Member Name: {eob.MemberName}");
                            c.Item().Text($"Address: {eob.Address}, {eob.City}, {eob.State} {eob.Zip}");
                            c.Item().Text($"Group Number: {eob.GroupNumber}");
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text($"Claim Number: {eob.ClaimNumber}");
                            c.Item().Text($"Date Paid: {(eob.DatePaid.HasValue ? eob.DatePaid.Value.ToString("MM/dd/yyyy") : "-")}");
                            c.Item().Text($"Customer Service: {eob.CustomerServicePhone}");
                        });
                    });
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text($"Provider: {eob.ProviderName}");
                        row.RelativeItem().Text($"Payee: {eob.PayeeName}");
                        row.RelativeItem().Text($"Date Received: {eob.DateReceived:MM/dd/yyyy}");
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(10).Table(table =>
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
                        void HeaderCell(string text) => header.Cell().Element(HeaderStyle).Text(text).Bold().FontSize(7);
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

                    foreach (var line in eob.Lines)
                    {
                        table.Cell().Element(BodyStyle).Text(line.LineNo.ToString());
                        table.Cell().Element(BodyStyle).Text(line.DateOfService.ToString("MM/dd/yy"));
                        table.Cell().Element(BodyStyle).Text(line.ServiceDescription);
                        table.Cell().Element(BodyStyle).Text(line.ClaimStatus);
                        table.Cell().Element(BodyStyle).AlignRight().Text($"${line.ProviderCharges:0.00}");
                        table.Cell().Element(BodyStyle).AlignRight().Text($"${line.AllowedCharges:0.00}");
                        table.Cell().Element(BodyStyle).AlignRight().Text($"${line.Copay:0.00}");
                        table.Cell().Element(BodyStyle).AlignRight().Text($"${line.Deductible:0.00}");
                        table.Cell().Element(BodyStyle).AlignRight().Text($"${line.Coinsurance:0.00}");
                        table.Cell().Element(BodyStyle).AlignRight().Text($"${line.PaidByInsurer:0.00}");
                        table.Cell().Element(BodyStyle).AlignRight().Text($"${line.WhatYouOwe:0.00}");
                        table.Cell().Element(BodyStyle).Text(line.RemarkCode ?? "");
                    }

                    var t = eob.Totals;
                    table.Cell().ColumnSpan(4).Element(TotalStyle).AlignRight().Text("Total").Bold();
                    table.Cell().Element(TotalStyle).AlignRight().Text($"${t.ProviderCharges:0.00}").Bold();
                    table.Cell().Element(TotalStyle).AlignRight().Text($"${t.AllowedCharges:0.00}").Bold();
                    table.Cell().Element(TotalStyle).AlignRight().Text($"${t.Copay:0.00}").Bold();
                    table.Cell().Element(TotalStyle).AlignRight().Text($"${t.Deductible:0.00}").Bold();
                    table.Cell().Element(TotalStyle).AlignRight().Text($"${t.Coinsurance:0.00}").Bold();
                    table.Cell().Element(TotalStyle).AlignRight().Text($"${t.PaidByInsurer:0.00}").Bold();
                    table.Cell().Element(TotalStyle).AlignRight().Text($"${t.WhatYouOwe:0.00}").Bold();
                    table.Cell().Element(TotalStyle).Text("");
                });

                page.Footer().PaddingTop(10).Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Text("Remark Code: PDC — Billed amount is higher than the maximum payment insurance allows. The payment is for the allowed amount.").FontSize(7).FontColor(Colors.Grey.Darken2);
                    col.Item().Text("An EOB is NOT A BILL. You may receive a separate bill from the provider.").FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            });
        }).GeneratePdf();
    }

    private static IContainer HeaderStyle(IContainer container) =>
        container.Background(Colors.Grey.Lighten3).Padding(3).BorderBottom(1).BorderColor(Colors.Grey.Darken1);

    private static IContainer BodyStyle(IContainer container) =>
        container.Padding(3).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2);

    private static IContainer TotalStyle(IContainer container) =>
        container.Background(Colors.Grey.Lighten2).Padding(3).BorderTop(1).BorderColor(Colors.Grey.Darken1);
}
