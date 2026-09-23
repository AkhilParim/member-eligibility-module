namespace MemberEligibility.Api.Models;

public class Enrollment
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public long PlanId { get; set; }
    public string SubscriberNumber { get; set; } = string.Empty;
    public string Relationship { get; set; } = "subscriber";
    public DateOnly EffectiveDate { get; set; }
    public DateOnly? TermDate { get; set; }
    public string Status { get; set; } = "active";
    public string? PcpName { get; set; }
    public string? PcpPhone { get; set; }

    public Member? Member { get; set; }
    public Plan? Plan { get; set; }
}
