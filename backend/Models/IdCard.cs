namespace MemberEligibility.Api.Models;

public class IdCard
{
    public long Id { get; set; }
    public long MemberId { get; set; }
    public long EnrollmentId { get; set; }
    public string? RxBin { get; set; }
    public string? RxPcn { get; set; }
    public string? RxGrp { get; set; }
    public string? RxId { get; set; }
    public DateOnly IssuedDate { get; set; }
    public string CardStatus { get; set; } = "active";
}
