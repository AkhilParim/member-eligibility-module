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

                page.Content().PaddingTop(10).Element(c => EobLinesTable.Compose(c, eob.Lines, eob.Totals));

                page.Footer().PaddingTop(10).Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Text("Remark Code: PDC — Billed amount is higher than the maximum payment insurance allows. The payment is for the allowed amount.").FontSize(7).FontColor(Colors.Grey.Darken2);
                    col.Item().Text("An EOB is NOT A BILL. You may receive a separate bill from the provider.").FontSize(7).FontColor(Colors.Grey.Darken2);
                });
            });
        }).GeneratePdf();
    }
}
