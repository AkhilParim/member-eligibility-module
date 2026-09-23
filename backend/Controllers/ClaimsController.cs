using Microsoft.AspNetCore.Mvc;
using MemberEligibility.Api.Dtos;
using MemberEligibility.Api.Pdf;
using MemberEligibility.Api.Services;

namespace MemberEligibility.Api.Controllers;

[ApiController]
[Route("api/claims")]
public class ClaimsController : ControllerBase
{
    private readonly EligibilityFunctions _functions;

    public ClaimsController(EligibilityFunctions functions)
    {
        _functions = functions;
    }

    [HttpGet("{claimId:long}/eob")]
    public async Task<ActionResult<EobDto>> GetEob(long claimId)
    {
        var eob = await _functions.GetEobAsync(claimId);
        if (eob is null) return NotFound();
        return Ok(eob);
    }

    [HttpGet("{claimId:long}/eob/pdf")]
    public async Task<IActionResult> GetEobPdf(long claimId)
    {
        var eob = await _functions.GetEobAsync(claimId);
        if (eob is null) return NotFound();

        var pdf = EobPdfGenerator.Generate(eob);
        return File(pdf, "application/pdf", $"eob-{eob.ClaimNumber}.pdf");
    }
}
