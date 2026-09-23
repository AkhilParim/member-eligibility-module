using System.Text.Json;
using Npgsql;
using MemberEligibility.Api.Dtos;

namespace MemberEligibility.Api.Services;

public class EligibilityFunctions
{
    private readonly NpgsqlDataSource _dataSource;

    public EligibilityFunctions(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<List<EligibilityDto>> GetEligibilityAsync(long memberId, DateOnly asOf)
    {
        await using var cmd = _dataSource.CreateCommand(
            "SELECT * FROM fn_get_member_eligibility($1, $2)");
        cmd.Parameters.AddWithValue(memberId);
        cmd.Parameters.AddWithValue(asOf);

        var results = new List<EligibilityDto>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new EligibilityDto(
                MemberId: reader.GetInt64(reader.GetOrdinal("member_id")),
                FirstName: reader.GetString(reader.GetOrdinal("first_name")),
                LastName: reader.GetString(reader.GetOrdinal("last_name")),
                DateOfBirth: reader.GetFieldValue<DateOnly>(reader.GetOrdinal("date_of_birth")),
                EnrollmentId: reader.GetInt64(reader.GetOrdinal("enrollment_id")),
                SubscriberNumber: reader.GetString(reader.GetOrdinal("subscriber_number")),
                Relationship: reader.GetString(reader.GetOrdinal("relationship")),
                EffectiveDate: reader.GetFieldValue<DateOnly>(reader.GetOrdinal("effective_date")),
                TermDate: reader.IsDBNull(reader.GetOrdinal("term_date")) ? null : reader.GetFieldValue<DateOnly>(reader.GetOrdinal("term_date")),
                EligibilityStatus: reader.GetString(reader.GetOrdinal("eligibility_status")),
                PlanId: reader.GetInt64(reader.GetOrdinal("plan_id")),
                PlanName: reader.GetString(reader.GetOrdinal("plan_name")),
                PlanType: reader.GetString(reader.GetOrdinal("plan_type")),
                GroupNumber: reader.GetString(reader.GetOrdinal("group_number")),
                PcpName: reader.IsDBNull(reader.GetOrdinal("pcp_name")) ? null : reader.GetString(reader.GetOrdinal("pcp_name")),
                PcpPhone: reader.IsDBNull(reader.GetOrdinal("pcp_phone")) ? null : reader.GetString(reader.GetOrdinal("pcp_phone")),
                CopayPcp: reader.GetDecimal(reader.GetOrdinal("copay_pcp")),
                CopaySpecialist: reader.GetDecimal(reader.GetOrdinal("copay_specialist")),
                CopayEr: reader.GetDecimal(reader.GetOrdinal("copay_er"))
            ));
        }
        return results;
    }

    public async Task<IdCardDataDto?> GetIdCardDataAsync(long memberId)
    {
        await using var cmd = _dataSource.CreateCommand(
            "SELECT * FROM fn_get_id_card_data($1)");
        cmd.Parameters.AddWithValue(memberId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new IdCardDataDto(
            CardId: reader.GetInt64(reader.GetOrdinal("card_id")),
            MemberId: reader.GetInt64(reader.GetOrdinal("member_id")),
            MemberName: reader.GetString(reader.GetOrdinal("member_name")),
            DateOfBirth: reader.GetFieldValue<DateOnly>(reader.GetOrdinal("date_of_birth")),
            SubscriberNumber: reader.GetString(reader.GetOrdinal("subscriber_number")),
            PlanName: reader.GetString(reader.GetOrdinal("plan_name")),
            PlanType: reader.GetString(reader.GetOrdinal("plan_type")),
            GroupNumber: reader.GetString(reader.GetOrdinal("group_number")),
            PayerId: reader.GetString(reader.GetOrdinal("payer_id")),
            CmsContractNumber: reader.IsDBNull(reader.GetOrdinal("cms_contract_number")) ? null : reader.GetString(reader.GetOrdinal("cms_contract_number")),
            PlanBenefitPackageNumber: reader.IsDBNull(reader.GetOrdinal("plan_benefit_package_number")) ? null : reader.GetString(reader.GetOrdinal("plan_benefit_package_number")),
            CustomerServicePhone: reader.IsDBNull(reader.GetOrdinal("customer_service_phone")) ? null : reader.GetString(reader.GetOrdinal("customer_service_phone")),
            MemberServicesPhone: reader.IsDBNull(reader.GetOrdinal("member_services_phone")) ? null : reader.GetString(reader.GetOrdinal("member_services_phone")),
            BehavioralHealthPhone: reader.IsDBNull(reader.GetOrdinal("behavioral_health_phone")) ? null : reader.GetString(reader.GetOrdinal("behavioral_health_phone")),
            PharmacyHelpDeskPhone: reader.IsDBNull(reader.GetOrdinal("pharmacy_help_desk_phone")) ? null : reader.GetString(reader.GetOrdinal("pharmacy_help_desk_phone")),
            Website: reader.IsDBNull(reader.GetOrdinal("website")) ? null : reader.GetString(reader.GetOrdinal("website")),
            ClaimsAddress: reader.IsDBNull(reader.GetOrdinal("claims_address")) ? null : reader.GetString(reader.GetOrdinal("claims_address")),
            ClaimInquiryPhone: reader.IsDBNull(reader.GetOrdinal("claim_inquiry_phone")) ? null : reader.GetString(reader.GetOrdinal("claim_inquiry_phone")),
            CopayPcp: reader.GetDecimal(reader.GetOrdinal("copay_pcp")),
            CopaySpecialist: reader.GetDecimal(reader.GetOrdinal("copay_specialist")),
            CopayEr: reader.GetDecimal(reader.GetOrdinal("copay_er")),
            EffectiveDate: reader.GetFieldValue<DateOnly>(reader.GetOrdinal("effective_date")),
            RxBin: reader.IsDBNull(reader.GetOrdinal("rx_bin")) ? null : reader.GetString(reader.GetOrdinal("rx_bin")),
            RxPcn: reader.IsDBNull(reader.GetOrdinal("rx_pcn")) ? null : reader.GetString(reader.GetOrdinal("rx_pcn")),
            RxGrp: reader.IsDBNull(reader.GetOrdinal("rx_grp")) ? null : reader.GetString(reader.GetOrdinal("rx_grp")),
            RxId: reader.IsDBNull(reader.GetOrdinal("rx_id")) ? null : reader.GetString(reader.GetOrdinal("rx_id")),
            IssuedDate: reader.GetFieldValue<DateOnly>(reader.GetOrdinal("issued_date"))
        );
    }

    public async Task<EobDto?> GetEobAsync(long claimId)
    {
        await using var cmd = _dataSource.CreateCommand("SELECT fn_get_eob($1)");
        cmd.Parameters.AddWithValue(claimId);

        var json = (string?)await cmd.ExecuteScalarAsync();
        if (string.IsNullOrEmpty(json)) return null;

        return JsonSerializer.Deserialize<EobDto>(json);
    }

    public async Task<long> EnrollMemberAsync(long memberId, long planId, string subscriberNumber,
        string relationship, DateOnly effectiveDate, string? pcpName, string? pcpPhone)
    {
        await using var cmd = _dataSource.CreateCommand(
            "SELECT sp_enroll_member($1, $2, $3, $4, $5, $6, $7)");
        cmd.Parameters.AddWithValue(memberId);
        cmd.Parameters.AddWithValue(planId);
        cmd.Parameters.AddWithValue(subscriberNumber);
        cmd.Parameters.AddWithValue(relationship);
        cmd.Parameters.AddWithValue(effectiveDate);
        cmd.Parameters.AddWithValue((object?)pcpName ?? DBNull.Value);
        cmd.Parameters.AddWithValue((object?)pcpPhone ?? DBNull.Value);

        return (long)(await cmd.ExecuteScalarAsync())!;
    }
}
