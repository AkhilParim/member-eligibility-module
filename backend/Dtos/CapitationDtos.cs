using System.Text.Json.Serialization;

namespace MemberEligibility.Api.Dtos;

public record CapitationTotalsDto(
    [property: JsonPropertyName("memberCount")] int MemberCount,
    [property: JsonPropertyName("memberMonths")] int MemberMonths,
    [property: JsonPropertyName("mmrPremium")] decimal MmrPremium,
    [property: JsonPropertyName("capitation")] decimal Capitation,
    [property: JsonPropertyName("capitationPercentOfPremium")] decimal? CapitationPercentOfPremium
);

public record CapitationContractDto(
    [property: JsonPropertyName("contractId")] long ContractId,
    [property: JsonPropertyName("contractNumber")] string ContractNumber,
    [property: JsonPropertyName("providerGroupName")] string ProviderGroupName,
    [property: JsonPropertyName("providerTin")] string? ProviderTin,
    [property: JsonPropertyName("pmpmRate")] decimal PmpmRate,
    [property: JsonPropertyName("effectiveDate")] DateOnly EffectiveDate,
    [property: JsonPropertyName("termDate")] DateOnly? TermDate
);

public record CapitationMemberDto(
    [property: JsonPropertyName("memberId")] long MemberId,
    [property: JsonPropertyName("memberName")] string MemberName,
    [property: JsonPropertyName("mbi")] string? Mbi,
    [property: JsonPropertyName("riskFactor")] decimal? RiskFactor,
    [property: JsonPropertyName("regularMonths")] int RegularMonths,
    [property: JsonPropertyName("adjustmentMonths")] int AdjustmentMonths,
    [property: JsonPropertyName("memberMonths")] int MemberMonths,
    [property: JsonPropertyName("mmrPremium")] decimal MmrPremium,
    [property: JsonPropertyName("capitation")] decimal Capitation
);

public record CapitationDetailDto(
    [property: JsonPropertyName("paymentMonth")] DateOnly PaymentMonth,
    [property: JsonPropertyName("contract")] CapitationContractDto Contract,
    [property: JsonPropertyName("members")] List<CapitationMemberDto> Members,
    [property: JsonPropertyName("totals")] CapitationTotalsDto Totals
);

public record CapitationSummaryContractDto(
    [property: JsonPropertyName("contractId")] long ContractId,
    [property: JsonPropertyName("contractNumber")] string ContractNumber,
    [property: JsonPropertyName("providerGroupName")] string ProviderGroupName,
    [property: JsonPropertyName("pmpmRate")] decimal PmpmRate,
    [property: JsonPropertyName("memberCount")] int MemberCount,
    [property: JsonPropertyName("memberMonths")] int MemberMonths,
    [property: JsonPropertyName("mmrPremium")] decimal MmrPremium,
    [property: JsonPropertyName("capitation")] decimal Capitation,
    [property: JsonPropertyName("capitationPercentOfPremium")] decimal? CapitationPercentOfPremium
);

public record CapitationSummaryDto(
    [property: JsonPropertyName("paymentMonth")] DateOnly PaymentMonth,
    [property: JsonPropertyName("contracts")] List<CapitationSummaryContractDto> Contracts,
    [property: JsonPropertyName("totals")] CapitationTotalsDto Totals
);
