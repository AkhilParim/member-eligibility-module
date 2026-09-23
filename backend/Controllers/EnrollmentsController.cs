using Microsoft.AspNetCore.Mvc;
using MemberEligibility.Api.Services;

namespace MemberEligibility.Api.Controllers;

public record EnrollMemberRequest(
    long MemberId, long PlanId, string SubscriberNumber, string Relationship,
    DateOnly EffectiveDate, string? PcpName, string? PcpPhone);

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController : ControllerBase
{
    private readonly EligibilityFunctions _functions;

    public EnrollmentsController(EligibilityFunctions functions)
    {
        _functions = functions;
    }

    [HttpPost]
    public async Task<ActionResult<long>> Enroll([FromBody] EnrollMemberRequest request)
    {
        var newEnrollmentId = await _functions.EnrollMemberAsync(
            request.MemberId, request.PlanId, request.SubscriberNumber,
            request.Relationship, request.EffectiveDate, request.PcpName, request.PcpPhone);

        return Ok(new { enrollmentId = newEnrollmentId });
    }
}
