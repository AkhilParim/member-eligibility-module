namespace MemberEligibility.Api.Models;

public class Claim
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public long EnrollmentId { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string PayeeName { get; set; } = string.Empty;
    public DateOnly DateReceived { get; set; }
    public DateOnly? DatePaid { get; set; }
    public string ClaimStatus { get; set; } = "paid";
    public DateOnly StatementDate { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;

    public List<ClaimLine> Lines { get; set; } = new();
}
