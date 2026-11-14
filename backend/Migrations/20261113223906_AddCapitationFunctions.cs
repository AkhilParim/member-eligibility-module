using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    public partial class AddCapitationFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- Flat PMPM capitation for one provider contract and payment month.
                -- Member-months come from the members' MMR rows (regular row = +1, retro adjustment = its own +/- count).
                -- Returns NULL when the contract does not exist or is not in effect that month.
                -- MMR premium (Part A + B) is informational only and is not used in the calculation.
                CREATE OR REPLACE FUNCTION fn_calculate_capitation(p_contract_id BIGINT, p_month DATE)
                RETURNS JSON AS $$
                DECLARE
                    v_month DATE := date_trunc('month', p_month)::date;
                    v_end   DATE := (date_trunc('month', p_month) + INTERVAL '1 month')::date - 1;
                    result  JSON;
                BEGIN
                    WITH the_contract AS (
                        SELECT *
                        FROM provider_contracts
                        WHERE id = p_contract_id
                          AND effective_date <= v_end
                          AND (term_date IS NULL OR term_date >= v_month)
                    ),
                    member_lines AS (
                        SELECT m.id AS member_id,
                               m.first_name || ' ' || m.last_name AS member_name,
                               m.mbi,
                               MAX(r.part_c_risk_factor) FILTER (WHERE r.adjustment_reason_code IS NULL) AS risk_factor,
                               COALESCE(SUM(r.member_months) FILTER (WHERE r.adjustment_reason_code IS NULL), 0)::int AS regular_months,
                               COALESCE(SUM(r.member_months) FILTER (WHERE r.adjustment_reason_code IS NOT NULL), 0)::int AS adjustment_months,
                               SUM(r.member_months)::int AS member_months,
                               SUM(r.part_a_payment + r.part_b_payment) AS mmr_premium,
                               ROUND(c.pmpm_rate * SUM(r.member_months), 2) AS capitation
                        FROM the_contract c
                        JOIN members m ON m.provider_contract_id = c.id
                        JOIN mmr_records r ON r.beneficiary_id = m.mbi AND r.payment_month = v_month
                        GROUP BY m.id, m.first_name, m.last_name, m.mbi, c.pmpm_rate
                    )
                    SELECT json_build_object(
                        'paymentMonth', v_month,
                        'contract', json_build_object(
                            'contractId', c.id,
                            'contractNumber', c.contract_number,
                            'providerGroupName', c.provider_group_name,
                            'providerTin', c.provider_tin,
                            'pmpmRate', c.pmpm_rate,
                            'effectiveDate', c.effective_date,
                            'termDate', c.term_date
                        ),
                        'members', COALESCE((
                            SELECT json_agg(json_build_object(
                                'memberId', l.member_id,
                                'memberName', l.member_name,
                                'mbi', l.mbi,
                                'riskFactor', l.risk_factor,
                                'regularMonths', l.regular_months,
                                'adjustmentMonths', l.adjustment_months,
                                'memberMonths', l.member_months,
                                'mmrPremium', l.mmr_premium,
                                'capitation', l.capitation
                            ) ORDER BY l.member_name)
                            FROM member_lines l
                        ), '[]'::json),
                        'totals', (
                            SELECT json_build_object(
                                'memberCount', COUNT(*),
                                'memberMonths', COALESCE(SUM(member_months), 0),
                                'mmrPremium', COALESCE(SUM(mmr_premium), 0),
                                'capitation', COALESCE(SUM(capitation), 0),
                                'capitationPercentOfPremium', ROUND(SUM(capitation) * 100 / NULLIF(SUM(mmr_premium), 0), 2)
                            )
                            FROM member_lines
                        )
                    )
                    INTO result
                    FROM the_contract c;

                    RETURN result;
                END;
                $$ LANGUAGE plpgsql;

                -- Capitation for every provider contract in effect in the payment month.
                CREATE OR REPLACE FUNCTION fn_capitation_summary(p_month DATE)
                RETURNS JSON AS $$
                DECLARE
                    v_month DATE := date_trunc('month', p_month)::date;
                    v_end   DATE := (date_trunc('month', p_month) + INTERVAL '1 month')::date - 1;
                    result  JSON;
                BEGIN
                    WITH active AS (
                        SELECT *
                        FROM provider_contracts
                        WHERE effective_date <= v_end
                          AND (term_date IS NULL OR term_date >= v_month)
                    ),
                    member_lines AS (
                        SELECT c.id AS contract_id,
                               m.id AS member_id,
                               SUM(r.member_months) AS member_months,
                               SUM(r.part_a_payment + r.part_b_payment) AS mmr_premium,
                               ROUND(c.pmpm_rate * SUM(r.member_months), 2) AS capitation
                        FROM active c
                        JOIN members m ON m.provider_contract_id = c.id
                        JOIN mmr_records r ON r.beneficiary_id = m.mbi AND r.payment_month = v_month
                        GROUP BY c.id, c.pmpm_rate, m.id
                    ),
                    per_contract AS (
                        SELECT c.id AS contract_id, c.contract_number, c.provider_group_name, c.pmpm_rate,
                               COUNT(l.member_id)::int AS member_count,
                               COALESCE(SUM(l.member_months), 0)::int AS member_months,
                               COALESCE(SUM(l.mmr_premium), 0) AS mmr_premium,
                               COALESCE(SUM(l.capitation), 0) AS capitation
                        FROM active c
                        LEFT JOIN member_lines l ON l.contract_id = c.id
                        GROUP BY c.id, c.contract_number, c.provider_group_name, c.pmpm_rate
                    )
                    SELECT json_build_object(
                        'paymentMonth', v_month,
                        'contracts', COALESCE((
                            SELECT json_agg(json_build_object(
                                'contractId', p.contract_id,
                                'contractNumber', p.contract_number,
                                'providerGroupName', p.provider_group_name,
                                'pmpmRate', p.pmpm_rate,
                                'memberCount', p.member_count,
                                'memberMonths', p.member_months,
                                'mmrPremium', p.mmr_premium,
                                'capitation', p.capitation,
                                'capitationPercentOfPremium', ROUND(p.capitation * 100 / NULLIF(p.mmr_premium, 0), 2)
                            ) ORDER BY p.contract_number)
                            FROM per_contract p
                        ), '[]'::json),
                        'totals', (
                            SELECT json_build_object(
                                'memberCount', COALESCE(SUM(member_count), 0),
                                'memberMonths', COALESCE(SUM(member_months), 0),
                                'mmrPremium', COALESCE(SUM(mmr_premium), 0),
                                'capitation', COALESCE(SUM(capitation), 0),
                                'capitationPercentOfPremium', ROUND(SUM(capitation) * 100 / NULLIF(SUM(mmr_premium), 0), 2)
                            )
                            FROM per_contract
                        )
                    )
                    INTO result;

                    RETURN result;
                END;
                $$ LANGUAGE plpgsql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS fn_capitation_summary(DATE);
                DROP FUNCTION IF EXISTS fn_calculate_capitation(BIGINT, DATE);
                """);
        }
    }
}
