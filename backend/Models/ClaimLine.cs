namespace MemberEligibility.Api.Models;

public class ClaimLine
{
    public long Id { get; set; }
    public long ClaimId { get; set; }
    public int LineNo { get; set; }
    public DateOnly DateOfService { get; set; }
    public string ServiceDescription { get; set; } = string.Empty;
    public string ClaimStatus { get; set; } = "Paid";
    public decimal ProviderCharges { get; set; }
    public decimal AllowedCharges { get; set; }
    public decimal Copay { get; set; }
    public decimal Deductible { get; set; }
    public decimal Coinsurance { get; set; }
    public decimal PaidByInsurer { get; set; }
    public decimal WhatYouOwe { get; set; }
    public string? RemarkCode { get; set; }
}
