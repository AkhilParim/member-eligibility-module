namespace MemberEligibility.Api.Models;

public class Plan
{
    public long Id { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public string GroupNumber { get; set; } = string.Empty;
    public string PayerId { get; set; } = string.Empty;
    public string? CmsContractNumber { get; set; }
    public string? PlanBenefitPackageNumber { get; set; }
    public string? CustomerServicePhone { get; set; }
    public string? MemberServicesPhone { get; set; }
    public string? BehavioralHealthPhone { get; set; }
    public string? PharmacyHelpDeskPhone { get; set; }
    public string? Website { get; set; }
    public string? ClaimsAddress { get; set; }
    public string? ClaimInquiryPhone { get; set; }
    public decimal CopayPcp { get; set; }
    public decimal CopaySpecialist { get; set; }
    public decimal CopayEr { get; set; }
}
