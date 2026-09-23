namespace MemberEligibility.Api.Dtos;

public record MemberSearchResultDto(
    long MemberId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? PlanName,
    string EligibilityStatus
);

public record EligibilityDto(
    long MemberId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    long EnrollmentId,
    string SubscriberNumber,
    string Relationship,
    DateOnly EffectiveDate,
    DateOnly? TermDate,
    string EligibilityStatus,
    long PlanId,
    string PlanName,
    string PlanType,
    string GroupNumber,
    string? PcpName,
    string? PcpPhone,
    decimal CopayPcp,
    decimal CopaySpecialist,
    decimal CopayEr
);

public record IdCardDataDto(
    long CardId,
    long MemberId,
    string MemberName,
    DateOnly DateOfBirth,
    string SubscriberNumber,
    string PlanName,
    string PlanType,
    string GroupNumber,
    string PayerId,
    string? CmsContractNumber,
    string? PlanBenefitPackageNumber,
    string? CustomerServicePhone,
    string? MemberServicesPhone,
    string? BehavioralHealthPhone,
    string? PharmacyHelpDeskPhone,
    string? Website,
    string? ClaimsAddress,
    string? ClaimInquiryPhone,
    decimal CopayPcp,
    decimal CopaySpecialist,
    decimal CopayEr,
    DateOnly EffectiveDate,
    string? RxBin,
    string? RxPcn,
    string? RxGrp,
    string? RxId,
    DateOnly IssuedDate
);

public record ClaimSummaryDto(
    long ClaimId,
    string ClaimNumber,
    string ProviderName,
    DateOnly DateReceived,
    DateOnly? DatePaid,
    string ClaimStatus,
    decimal TotalBilled,
    decimal TotalWhatYouOwe
);
