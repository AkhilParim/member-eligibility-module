using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MemberEligibility.Api.Dtos;
using static MemberEligibility.Api.Pdf.PdfStyles;

namespace MemberEligibility.Api.Pdf;

// One EOB section per claim, using the same layout as the CMS sample EOB.
public static class MemberEobPdfGenerator
{
    public static byte[] Generate(MemberEobDto eob)
    {
        var legend = CodeGlossary.For(eob.Claims.SelectMany(c => c.Lines).Select(l => l.RemarkCode));

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
                            c.Item().Text($"Statement Date: {Date(eob.StatementDate)}");
                            c.Item().Text($"Service Period: {Period(eob.PeriodFrom, eob.PeriodTo)}");
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
                            c.Item().Text($"Plan: {eob.PlanName}");
                            c.Item().Text($"Customer Service: {eob.CustomerServicePhone}");
                            c.Item().Text($"Claims on this statement: {eob.Claims.Count}");
                        });
                    });
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(10).Column(col =>
                {
                    col.Spacing(14);

                    foreach (var claim in eob.Claims)
                        col.Item().ShowEntire().Element(c => ClaimSection(c, claim));

                    col.Item().ShowEntire().Element(c => StatementTotals(c, eob));

                    if (legend.Count > 0)
                        col.Item().Element(c => CodeGlossary.Compose(c, legend));
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text("An EOB is NOT A BILL. You may receive a separate bill from the provider.")
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

    private static void ClaimSection(IContainer container, MemberEobClaimDto claim)
    {
        container.Column(col =>
        {
            col.Item().Background(Colors.Grey.Lighten4).Padding(4).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Claim Number: {claim.ClaimNumber}").Bold();
                    c.Item().Text($"Document Number: {claim.DocumentNumber}");
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Provider: {claim.ProviderName}");
                    c.Item().Text($"Payee: {claim.PayeeName}");
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Date Received: {Date(claim.DateReceived)}");
                    c.Item().Text($"Date Paid: {Date(claim.DatePaid)}   Status: {claim.ClaimStatus}");
                });
            });

            col.Item().PaddingTop(4).Element(c => EobLinesTable.Compose(c, claim.Lines, claim.Totals));
        });
    }

    private static void StatementTotals(IContainer container, MemberEobDto eob)
    {
        var t = eob.Totals;
        container.Column(col =>
        {
            col.Item().Text($"STATEMENT TOTALS ({eob.Claims.Count} claims)").Bold().FontSize(10);
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < 7; i++) columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    void H(string text) => header.Cell().Element(HeaderStyle).AlignRight().Text(text).Bold().FontSize(7);
                    H("Provider Charges");
                    H("Allowed Charges");
                    H("CoPay");
                    H("Deductible");
                    H("Coinsurance");
                    H("Paid by Insurer");
                    H("What You Owe");
                });

                void V(decimal value) => table.Cell().Element(TotalStyle).AlignRight().Text(Money(value)).Bold();
                V(t.ProviderCharges);
                V(t.AllowedCharges);
                V(t.Copay);
                V(t.Deductible);
                V(t.Coinsurance);
                V(t.PaidByInsurer);
                V(t.WhatYouOwe);
            });
        });
    }
}
