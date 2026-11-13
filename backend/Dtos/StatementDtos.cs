using System.Text.Json.Serialization;

namespace MemberEligibility.Api.Dtos;

public record MemberEobClaimDto(
    [property: JsonPropertyName("claimId")] long ClaimId,
    [property: JsonPropertyName("claimNumber")] string ClaimNumber,
    [property: JsonPropertyName("documentNumber")] string DocumentNumber,
    [property: JsonPropertyName("statementDate")] DateOnly StatementDate,
    [property: JsonPropertyName("dateReceived")] DateOnly DateReceived,
    [property: JsonPropertyName("datePaid")] DateOnly? DatePaid,
    [property: JsonPropertyName("claimStatus")] string ClaimStatus,
    [property: JsonPropertyName("providerName")] string ProviderName,
    [property: JsonPropertyName("payeeName")] string PayeeName,
    [property: JsonPropertyName("lines")] List<EobLineDto> Lines,
    [property: JsonPropertyName("totals")] EobTotalsDto Totals
);

public record MemberEobDto(
    [property: JsonPropertyName("memberId")] long MemberId,
    [property: JsonPropertyName("memberName")] string MemberName,
    [property: JsonPropertyName("address")] string? Address,
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("zip")] string? Zip,
    [property: JsonPropertyName("subscriberNumber")] string? SubscriberNumber,
    [property: JsonPropertyName("groupNumber")] string? GroupNumber,
    [property: JsonPropertyName("planName")] string? PlanName,
    [property: JsonPropertyName("customerServicePhone")] string? CustomerServicePhone,
    [property: JsonPropertyName("statementDate")] DateOnly StatementDate,
    [property: JsonPropertyName("periodFrom")] DateOnly? PeriodFrom,
    [property: JsonPropertyName("periodTo")] DateOnly? PeriodTo,
    [property: JsonPropertyName("claims")] List<MemberEobClaimDto> Claims,
    [property: JsonPropertyName("totals")] EobTotalsDto Totals
);

public record EopTotalsDto(
    [property: JsonPropertyName("billed")] decimal Billed,
    [property: JsonPropertyName("contractualAdjustment")] decimal ContractualAdjustment,
    [property: JsonPropertyName("allowed")] decimal Allowed,
    [property: JsonPropertyName("copay")] decimal Copay,
    [property: JsonPropertyName("deductible")] decimal Deductible,
    [property: JsonPropertyName("coinsurance")] decimal Coinsurance,
    [property: JsonPropertyName("patientResponsibility")] decimal PatientResponsibility,
    [property: JsonPropertyName("paid")] decimal Paid
);

public record EopLineDto(
    [property: JsonPropertyName("lineNo")] int LineNo,
    [property: JsonPropertyName("dateOfService")] DateOnly DateOfService,
    [property: JsonPropertyName("procedureCode")] string? ProcedureCode,
    [property: JsonPropertyName("serviceDescription")] string ServiceDescription,
    [property: JsonPropertyName("lineStatus")] string LineStatus,
    [property: JsonPropertyName("billed")] decimal Billed,
    [property: JsonPropertyName("contractualAdjustment")] decimal ContractualAdjustment,
    [property: JsonPropertyName("allowed")] decimal Allowed,
    [property: JsonPropertyName("copay")] decimal Copay,
    [property: JsonPropertyName("deductible")] decimal Deductible,
    [property: JsonPropertyName("coinsurance")] decimal Coinsurance,
    [property: JsonPropertyName("patientResponsibility")] decimal PatientResponsibility,
    [property: JsonPropertyName("paid")] decimal Paid,
    [property: JsonPropertyName("reasonCode")] string? ReasonCode,
    [property: JsonPropertyName("remarkCode")] string? RemarkCode
);

public record EopClaimDto(
    [property: JsonPropertyName("claimId")] long ClaimId,
    [property: JsonPropertyName("claimNumber")] string ClaimNumber,
    [property: JsonPropertyName("documentNumber")] string DocumentNumber,
    [property: JsonPropertyName("providerName")] string ProviderName,
    [property: JsonPropertyName("claimStatus")] string ClaimStatus,
    [property: JsonPropertyName("dateReceived")] DateOnly DateReceived,
    [property: JsonPropertyName("lines")] List<EopLineDto> Lines,
    [property: JsonPropertyName("totals")] EopTotalsDto Totals
);

public record EopPaymentDto(
    [property: JsonPropertyName("payeeName")] string PayeeName,
    [property: JsonPropertyName("providerNpi")] string? ProviderNpi,
    [property: JsonPropertyName("paymentNumber")] string? PaymentNumber,
    [property: JsonPropertyName("paymentMethod")] string? PaymentMethod,
    [property: JsonPropertyName("paymentDate")] DateOnly? PaymentDate,
    [property: JsonPropertyName("claimCount")] int ClaimCount,
    [property: JsonPropertyName("claims")] List<EopClaimDto> Claims,
    [property: JsonPropertyName("totals")] EopTotalsDto Totals
);

public record MemberEopDto(
    [property: JsonPropertyName("memberId")] long MemberId,
    [property: JsonPropertyName("memberName")] string MemberName,
    [property: JsonPropertyName("subscriberNumber")] string? SubscriberNumber,
    [property: JsonPropertyName("groupNumber")] string? GroupNumber,
    [property: JsonPropertyName("payerName")] string? PayerName,
    [property: JsonPropertyName("payerId")] string? PayerId,
    [property: JsonPropertyName("payerPhone")] string? PayerPhone,
    [property: JsonPropertyName("payerAddress")] string? PayerAddress,
    [property: JsonPropertyName("statementDate")] DateOnly StatementDate,
    [property: JsonPropertyName("periodFrom")] DateOnly? PeriodFrom,
    [property: JsonPropertyName("periodTo")] DateOnly? PeriodTo,
    [property: JsonPropertyName("claimCount")] int ClaimCount,
    [property: JsonPropertyName("payments")] List<EopPaymentDto> Payments,
    [property: JsonPropertyName("totals")] EopTotalsDto Totals
);
