namespace MemberEligibility.Api.Models;

public class ProviderContract
{
    public long Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string ProviderGroupName { get; set; } = string.Empty;
    public string? ProviderTin { get; set; }
    public decimal PmpmRate { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? TermDate { get; set; }
}
