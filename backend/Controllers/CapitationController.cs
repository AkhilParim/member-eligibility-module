using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberEligibility.Api.Data;
using MemberEligibility.Api.Dtos;
using MemberEligibility.Api.Pdf;
using MemberEligibility.Api.Services;

namespace MemberEligibility.Api.Controllers;

[ApiController]
[Route("api/capitation")]
public class CapitationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EligibilityFunctions _functions;

    public CapitationController(AppDbContext db, EligibilityFunctions functions)
    {
        _db = db;
        _functions = functions;
    }

    [HttpGet("months")]
    public async Task<ActionResult<List<DateOnly>>> GetMonths()
    {
        var months = await _db.MmrRecords
            .Select(r => r.PaymentMonth)
            .Distinct()
            .OrderByDescending(m => m)
            .ToListAsync();

        return Ok(months);
    }

    [HttpGet]
    public async Task<ActionResult<CapitationSummaryDto>> GetSummary([FromQuery] DateOnly? month)
    {
        if (month is null) return MonthRequiredProblem();

        var summary = await _functions.GetCapitationSummaryAsync(month.Value);
        if (summary is null || summary.Totals.MemberCount == 0) return NoDataProblem();

        return Ok(summary);
    }

    [HttpGet("{contractId:long}")]
    public async Task<ActionResult<CapitationDetailDto>> GetDetail(long contractId, [FromQuery] DateOnly? month)
    {
        if (month is null) return MonthRequiredProblem();

        var detail = await _functions.GetCapitationAsync(contractId, month.Value);
        if (detail is null || detail.Members.Count == 0) return NoDataProblem();

        return Ok(detail);
    }

    [HttpGet("{contractId:long}/pdf")]
    public async Task<IActionResult> GetPdf(long contractId, [FromQuery] DateOnly? month)
    {
        if (month is null) return MonthRequiredProblem();

        var detail = await _functions.GetCapitationAsync(contractId, month.Value);
        if (detail is null || detail.Members.Count == 0) return NoDataProblem();

        var fileName = $"capitation-{detail.Contract.ContractNumber}-{detail.PaymentMonth:yyyy-MM}.pdf";
        return File(CapitationPdfGenerator.Generate(detail), "application/pdf", fileName);
    }

    private ObjectResult MonthRequiredProblem() =>
        Problem(detail: "'month' is required, e.g. ?month=2025-09-01.", statusCode: StatusCodes.Status400BadRequest);

    private ObjectResult NoDataProblem() =>
        Problem(detail: "No MMR data found for that contract and month.", statusCode: StatusCodes.Status404NotFound);
}
