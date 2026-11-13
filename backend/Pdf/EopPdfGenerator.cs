using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MemberEligibility.Api.Dtos;
using static MemberEligibility.Api.Pdf.PdfStyles;

namespace MemberEligibility.Api.Pdf;

// Provider-facing remittance. Columns follow the CMS Standard Paper Remittance Advice
// (serv date, proc, billed, allowed, deduct, coins, group/reason-code amount, provider paid),
// with copay added since this plan data carries it separately.
public static class EopPdfGenerator
{
    private const int ColumnCount = 13;

    public static byte[] Generate(MemberEopDto eop)
    {
        var legend = CodeGlossary.For(
            eop.Payments
                .SelectMany(p => p.Claims)
                .SelectMany(c => c.Lines)
                .SelectMany(l => new[] { l.ReasonCode, l.RemarkCode }));

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter.Landscape());
                page.Margin(28);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Lato"));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem(2).Column(c =>
                        {
                            c.Item().Text("EXPLANATION OF PAYMENT").Bold().FontSize(16);
                            c.Item().Text("Remittance advice for processed claims. This is not a bill to the patient.")
                                .FontColor(Colors.Grey.Darken2);
                        });
                        row.RelativeItem(1).Column(c =>
                        {
                            c.Item().Text(eop.PayerName ?? "").Bold();
                            c.Item().Text($"Payer ID: {eop.PayerId}");
                            c.Item().Text($"Provider inquiries: {eop.PayerPhone}");
                            c.Item().Text(eop.PayerAddress ?? "");
                        });
                    });

                    col.Item().PaddingTop(6).Row(row =>
                    {
                        row.RelativeItem().Text($"Patient: {eop.MemberName}");
                        row.RelativeItem().Text($"Member ID: {eop.SubscriberNumber}");
                        row.RelativeItem().Text($"Group: {eop.GroupNumber}");
                        row.RelativeItem().Text($"Statement Date: {Date(eop.StatementDate)}");
                        row.RelativeItem().Text($"Service Period: {Period(eop.PeriodFrom, eop.PeriodTo)}");
                    });

                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(8).Column(col =>
                {
                    col.Spacing(12);

                    foreach (var payment in eop.Payments)
                        col.Item().EnsureSpace(130).Element(c => PaymentSection(c, payment));

                    col.Item().ShowEntire().Element(c => GrandTotals(c, eop));

                    if (legend.Count > 0)
                        col.Item().Element(c => CodeGlossary.Compose(c, legend));
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text("Amounts balance at line, claim and payment level: billed - adjustment = allowed; allowed - patient responsibility = provider paid.")
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

    private static void PaymentSection(IContainer container, EopPaymentDto payment)
    {
        container.Column(col =>
        {
            col.Item().Background(Colors.Blue.Lighten4).Padding(5).Row(row =>
            {
                row.RelativeItem(2).Column(c =>
                {
                    c.Item().Text($"Payee: {payment.PayeeName}").Bold();
                    c.Item().Text($"Provider NPI: {payment.ProviderNpi ?? "-"}");
                });
                row.RelativeItem(2).Column(c =>
                {
                    if (payment.PaymentNumber is null)
                    {
                        c.Item().Text("No payment issued").Bold().FontColor(Colors.Red.Darken2);
                        c.Item().Text($"Claims processed {Date(payment.PaymentDate)} with $0.00 payable");
                    }
                    else
                    {
                        c.Item().Text($"Payment: {payment.PaymentMethod} #{payment.PaymentNumber}").Bold();
                        c.Item().Text($"Payment Date: {Date(payment.PaymentDate)}");
                    }
                });
                row.RelativeItem(1).AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text("Payment Amount").FontSize(7);
                    c.Item().AlignRight().Text(Money(payment.Totals.Paid)).Bold().FontSize(11);
                });
            });

            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);  // serv date
                    columns.ConstantColumn(40);  // proc
                    columns.RelativeColumn();    // description
                    columns.ConstantColumn(52);  // billed
                    columns.ConstantColumn(52);  // adjustment (RC-AMT)
                    columns.ConstantColumn(36);  // group/reason code
                    columns.ConstantColumn(52);  // allowed
                    columns.ConstantColumn(42);  // copay
                    columns.ConstantColumn(46);  // deduct
                    columns.ConstantColumn(42);  // coins
                    columns.ConstantColumn(50);  // patient responsibility
                    columns.ConstantColumn(52);  // provider paid
                    columns.ConstantColumn(40);  // remark
                });

                table.Header(header =>
                {
                    void H(string text, bool right = true)
                    {
                        var cell = header.Cell().Element(HeaderStyle);
                        if (right) cell.AlignRight().Text(text).Bold().FontSize(7);
                        else cell.Text(text).Bold().FontSize(7);
                    }

                    H("Serv Date", false);
                    H("Proc", false);
                    H("Description", false);
                    H("Billed");
                    H("Adjustment");
                    H("RC");
                    H("Allowed");
                    H("CoPay");
                    H("Deduct");
                    H("Coins");
                    H("Pt Resp");
                    H("Prov Pd");
                    H("Remark");
                });

                foreach (var claim in payment.Claims)
                {
                    table.Cell().ColumnSpan(ColumnCount).Background(Colors.Grey.Lighten4).Padding(3)
                        .Text($"Claim {claim.ClaimNumber}   ·   Doc {claim.DocumentNumber}   ·   Rendering provider: {claim.ProviderName}   ·   Received {Date(claim.DateReceived)}   ·   Status: {claim.ClaimStatus}")
                        .SemiBold().FontSize(7.5f);

                    foreach (var line in claim.Lines)
                    {
                        var description = line.LineStatus.Equals("Denied", StringComparison.OrdinalIgnoreCase)
                            ? $"{line.ServiceDescription} (denied)"
                            : line.ServiceDescription;

                        table.Cell().Element(BodyStyle).Text(line.DateOfService.ToString("MM/dd/yy"));
                        table.Cell().Element(BodyStyle).Text(line.ProcedureCode ?? "-");
                        table.Cell().Element(BodyStyle).Text(description);
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.Billed));
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.ContractualAdjustment));
                        table.Cell().Element(BodyStyle).AlignRight().Text(line.ReasonCode ?? "");
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.Allowed));
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.Copay));
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.Deductible));
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.Coinsurance));
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.PatientResponsibility));
                        table.Cell().Element(BodyStyle).AlignRight().Text(Money(line.Paid));
                        table.Cell().Element(BodyStyle).AlignRight().Text(line.RemarkCode ?? "");
                    }

                    TotalsRow(table, "Claim totals", claim.Totals);
                }

                TotalsRow(table, $"Payment totals ({payment.ClaimCount} claim{(payment.ClaimCount == 1 ? "" : "s")})", payment.Totals);
            });
        });
    }

    private static void TotalsRow(TableDescriptor table, string label, EopTotalsDto t)
    {
        table.Cell().ColumnSpan(3).Element(TotalStyle).AlignRight().Text(label).Bold();
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.Billed)).Bold();
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.ContractualAdjustment)).Bold();
        table.Cell().Element(TotalStyle).Text("");
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.Allowed)).Bold();
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.Copay)).Bold();
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.Deductible)).Bold();
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.Coinsurance)).Bold();
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.PatientResponsibility)).Bold();
        table.Cell().Element(TotalStyle).AlignRight().Text(Money(t.Paid)).Bold();
        table.Cell().Element(TotalStyle).Text("");
    }

    private static void GrandTotals(IContainer container, MemberEopDto eop)
    {
        var t = eop.Totals;
        container.Column(col =>
        {
            col.Item().Text($"TOTALS: {eop.ClaimCount} claims in {eop.Payments.Count} payment group{(eop.Payments.Count == 1 ? "" : "s")}")
                .Bold().FontSize(10);
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (var i = 0; i < 8; i++) columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    void H(string text) => header.Cell().Element(HeaderStyle).AlignRight().Text(text).Bold().FontSize(7);
                    H("Billed");
                    H("Adjustments");
                    H("Allowed");
                    H("CoPay");
                    H("Deductible");
                    H("Coinsurance");
                    H("Patient Resp");
                    H("Provider Paid");
                });

                void V(decimal value) => table.Cell().Element(TotalStyle).AlignRight().Text(Money(value)).Bold();
                V(t.Billed);
                V(t.ContractualAdjustment);
                V(t.Allowed);
                V(t.Copay);
                V(t.Deductible);
                V(t.Coinsurance);
                V(t.PatientResponsibility);
                V(t.Paid);
            });
        });
    }
}
