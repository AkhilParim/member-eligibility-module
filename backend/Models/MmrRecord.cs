namespace MemberEligibility.Api.Models;

public class MmrRecord
{
    public long Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string PlanBenefitPackage { get; set; } = string.Empty;
    public string BeneficiaryId { get; set; } = string.Empty;
    public DateOnly PaymentMonth { get; set; }
    public string? AdjustmentReasonCode { get; set; }
    public int MemberMonths { get; set; }
    public decimal PartCRiskFactor { get; set; }
    public decimal PartAPayment { get; set; }
    public decimal PartBPayment { get; set; }
    public decimal PartDPayment { get; set; }
}
