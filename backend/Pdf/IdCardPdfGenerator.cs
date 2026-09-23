using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MemberEligibility.Api.Dtos;

namespace MemberEligibility.Api.Pdf;

// Field set/layout follows the CMS sample dual-eligible Member ID Card template.
public static class IdCardPdfGenerator
{
    public static byte[] Generate(IdCardDataDto card)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(400, 420, Unit.Point);
                page.Margin(16);
                page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

                page.Content().Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text(card.PlanName).Bold().FontSize(13);
                    });
                    col.Item().PaddingTop(2).Text(card.PlanType).FontColor(Colors.Grey.Darken2);

                    col.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem(1).Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Member Name: ").SemiBold(); t.Span(card.MemberName); });
                            c.Item().Text(t => { t.Span("Member ID: ").SemiBold(); t.Span(card.SubscriberNumber); });
                            c.Item().Text(t => { t.Span("Group Number: ").SemiBold(); t.Span(card.GroupNumber); });
                            c.Item().Text(t => { t.Span("Effective: ").SemiBold(); t.Span(card.EffectiveDate.ToString("MM/dd/yyyy")); });
                        });
                        row.RelativeItem(1).Column(c =>
                        {
                            c.Item().Text(t => { t.Span("RxBIN: ").SemiBold(); t.Span(card.RxBin ?? "-"); });
                            c.Item().Text(t => { t.Span("RxPCN: ").SemiBold(); t.Span(card.RxPcn ?? "-"); });
                            c.Item().Text(t => { t.Span("RxGRP: ").SemiBold(); t.Span(card.RxGrp ?? "-"); });
                            c.Item().Text(t => { t.Span("RxID: ").SemiBold(); t.Span(card.RxId ?? card.SubscriberNumber); });
                        });
                    });

                    col.Item().PaddingTop(8).Text(t =>
                    {
                        t.Span("Copays — PCP: ").SemiBold(); t.Span($"${card.CopayPcp:0.00}  ");
                        t.Span("Specialist: ").SemiBold(); t.Span($"${card.CopaySpecialist:0.00}  ");
                        t.Span("ER: ").SemiBold(); t.Span($"${card.CopayEr:0.00}");
                    });

                    col.Item().PaddingTop(6).Text(t =>
                    {
                        t.Span("CMS Contract #: ").SemiBold(); t.Span(card.CmsContractNumber ?? "-");
                        t.Span("   Plan Benefit Package #: ").SemiBold(); t.Span(card.PlanBenefitPackageNumber ?? "-");
                    });

                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

                    col.Item().PaddingTop(8).Text("Back of Card").Bold().FontSize(10);
                    col.Item().PaddingTop(4).Text(t => { t.Span("Member Services: ").SemiBold(); t.Span(card.MemberServicesPhone ?? "-"); });
                    col.Item().Text(t => { t.Span("Behavioral Health: ").SemiBold(); t.Span(card.BehavioralHealthPhone ?? "-"); });
                    col.Item().Text(t => { t.Span("Pharmacy Help Desk: ").SemiBold(); t.Span(card.PharmacyHelpDeskPhone ?? "-"); });
                    col.Item().Text(t => { t.Span("Claim Inquiry: ").SemiBold(); t.Span(card.ClaimInquiryPhone ?? "-"); });
                    col.Item().Text(t => { t.Span("Website: ").SemiBold(); t.Span(card.Website ?? "-"); });
                    col.Item().Text(t => { t.Span("Send Claims To: ").SemiBold(); t.Span(card.ClaimsAddress ?? "-"); });
                });
            });
        }).GeneratePdf();
    }
}
