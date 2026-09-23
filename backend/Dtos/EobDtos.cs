using System.Text.Json.Serialization;

namespace MemberEligibility.Api.Dtos;

public record EobLineDto(
    [property: JsonPropertyName("lineNo")] int LineNo,
    [property: JsonPropertyName("dateOfService")] DateOnly DateOfService,
    [property: JsonPropertyName("serviceDescription")] string ServiceDescription,
    [property: JsonPropertyName("claimStatus")] string ClaimStatus,
    [property: JsonPropertyName("providerCharges")] decimal ProviderCharges,
    [property: JsonPropertyName("allowedCharges")] decimal AllowedCharges,
    [property: JsonPropertyName("copay")] decimal Copay,
    [property: JsonPropertyName("deductible")] decimal Deductible,
    [property: JsonPropertyName("coinsurance")] decimal Coinsurance,
    [property: JsonPropertyName("paidByInsurer")] decimal PaidByInsurer,
    [property: JsonPropertyName("whatYouOwe")] decimal WhatYouOwe,
    [property: JsonPropertyName("remarkCode")] string? RemarkCode
);

public record EobTotalsDto(
    [property: JsonPropertyName("providerCharges")] decimal ProviderCharges,
    [property: JsonPropertyName("allowedCharges")] decimal AllowedCharges,
    [property: JsonPropertyName("copay")] decimal Copay,
    [property: JsonPropertyName("deductible")] decimal Deductible,
    [property: JsonPropertyName("coinsurance")] decimal Coinsurance,
    [property: JsonPropertyName("paidByInsurer")] decimal PaidByInsurer,
    [property: JsonPropertyName("whatYouOwe")] decimal WhatYouOwe
);

public record EobDto(
    [property: JsonPropertyName("claimId")] long ClaimId,
    [property: JsonPropertyName("claimNumber")] string ClaimNumber,
    [property: JsonPropertyName("documentNumber")] string DocumentNumber,
    [property: JsonPropertyName("statementDate")] DateOnly StatementDate,
    [property: JsonPropertyName("dateReceived")] DateOnly DateReceived,
    [property: JsonPropertyName("datePaid")] DateOnly? DatePaid,
    [property: JsonPropertyName("claimStatus")] string ClaimStatus,
    [property: JsonPropertyName("providerName")] string ProviderName,
    [property: JsonPropertyName("payeeName")] string PayeeName,
    [property: JsonPropertyName("memberId")] long MemberId,
    [property: JsonPropertyName("memberName")] string MemberName,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("zip")] string? Zip,
    [property: JsonPropertyName("subscriberNumber")] string SubscriberNumber,
    [property: JsonPropertyName("groupNumber")] string GroupNumber,
    [property: JsonPropertyName("customerServicePhone")] string? CustomerServicePhone,
    [property: JsonPropertyName("lines")] List<EobLineDto> Lines,
    [property: JsonPropertyName("totals")] EobTotalsDto Totals
);
