using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MemberEligibility.Api.Data;
using MemberEligibility.Api.Dtos;
using MemberEligibility.Api.Pdf;
using MemberEligibility.Api.Services;

namespace MemberEligibility.Api.Controllers;

[ApiController]
[Route("api/members")]
public class MembersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly EligibilityFunctions _functions;

    public MembersController(AppDbContext db, EligibilityFunctions functions)
    {
        _db = db;
        _functions = functions;
    }

    [HttpGet]
    public async Task<ActionResult<List<MemberSearchResultDto>>> Search([FromQuery] string? search)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _db.Members
            .Include(m => m.Enrollments)
            .ThenInclude(e => e.Plan)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(m =>
                EF.Functions.ILike(m.FirstName, $"%{term}%") ||
                EF.Functions.ILike(m.LastName, $"%{term}%") ||
                m.Enrollments.Any(e => e.SubscriberNumber == term));
        }

        var members = await query.OrderBy(m => m.LastName).ThenBy(m => m.FirstName).ToListAsync();

        var results = members.Select(m =>
        {
            var enrollment = m.Enrollments
                .OrderByDescending(e => e.EffectiveDate)
                .FirstOrDefault();

            var isEligible = enrollment is not null
                && enrollment.Status == "active"
                && today >= enrollment.EffectiveDate
                && (enrollment.TermDate is null || today <= enrollment.TermDate);

            return new MemberSearchResultDto(
                m.Id, m.FirstName, m.LastName, m.DateOfBirth,
                enrollment?.Plan?.PlanName,
                isEligible ? "eligible" : "not_eligible");
        }).ToList();

        return Ok(results);
    }

    [HttpGet("{memberId:long}/eligibility")]
    public async Task<ActionResult<List<EligibilityDto>>> GetEligibility(long memberId, [FromQuery] DateOnly? asOf)
    {
        var effectiveAsOf = asOf ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _functions.GetEligibilityAsync(memberId, effectiveAsOf);
        if (result.Count == 0) return NotFound();
        return Ok(result);
    }

    [HttpGet("{memberId:long}/idcard")]
    public async Task<ActionResult<IdCardDataDto>> GetIdCard(long memberId)
    {
        var card = await _functions.GetIdCardDataAsync(memberId);
        if (card is null) return NotFound();
        return Ok(card);
    }

    [HttpGet("{memberId:long}/idcard/pdf")]
    public async Task<IActionResult> GetIdCardPdf(long memberId)
    {
        var card = await _functions.GetIdCardDataAsync(memberId);
        if (card is null) return NotFound();

        var pdf = IdCardPdfGenerator.Generate(card);
        return File(pdf, "application/pdf", $"id-card-{memberId}.pdf");
    }

    [HttpGet("{memberId:long}/claims")]
    public async Task<ActionResult<List<ClaimSummaryDto>>> GetClaims(long memberId)
    {
        var claims = await _db.Claims
            .Include(c => c.Lines)
            .Where(c => c.MemberId == memberId)
            .OrderByDescending(c => c.DateReceived)
            .Select(c => new ClaimSummaryDto(
                c.Id, c.ClaimNumber, c.ProviderName, c.DateReceived, c.DatePaid, c.ClaimStatus,
                c.Lines.Sum(l => l.ProviderCharges),
                c.Lines.Sum(l => l.WhatYouOwe)))
            .ToListAsync();

        return Ok(claims);
    }
}
