using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    /// <inheritdoc />
    public partial class StoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION fn_get_member_eligibility(p_member_id BIGINT, p_as_of DATE DEFAULT CURRENT_DATE)
                RETURNS TABLE (
                    member_id           BIGINT,
                    first_name           VARCHAR,
                    last_name             VARCHAR,
                    date_of_birth          DATE,
                    enrollment_id            BIGINT,
                    subscriber_number         VARCHAR,
                    relationship               VARCHAR,
                    effective_date              DATE,
                    term_date                    DATE,
                    eligibility_status            VARCHAR,
                    plan_id                        BIGINT,
                    plan_name                       VARCHAR,
                    plan_type                        VARCHAR,
                    group_number                      VARCHAR,
                    pcp_name                           VARCHAR,
                    pcp_phone                           VARCHAR,
                    copay_pcp                            NUMERIC,
                    copay_specialist                      NUMERIC,
                    copay_er                               NUMERIC
                ) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT
                        m.id, m.first_name, m.last_name, m.date_of_birth,
                        e.id, e.subscriber_number, e.relationship, e.effective_date, e.term_date,
                        CASE
                            WHEN e.status = 'active'
                                 AND p_as_of >= e.effective_date
                                 AND (e.term_date IS NULL OR p_as_of <= e.term_date)
                            THEN 'eligible'
                            ELSE 'not_eligible'
                        END::VARCHAR AS eligibility_status,
                        p.id, p.plan_name, p.plan_type, p.group_number,
                        e.pcp_name, e.pcp_phone, p.copay_pcp, p.copay_specialist, p.copay_er
                    FROM members m
                    JOIN enrollments e ON e.member_id = m.id
                    JOIN plans p ON p.id = e.plan_id
                    WHERE m.id = p_member_id
                    ORDER BY e.effective_date DESC;
                END;
                $$ LANGUAGE plpgsql;

                CREATE OR REPLACE FUNCTION fn_get_id_card_data(p_member_id BIGINT)
                RETURNS TABLE (
                    card_id          BIGINT,
                    member_id         BIGINT,
                    member_name        VARCHAR,
                    date_of_birth        DATE,
                    subscriber_number      VARCHAR,
                    plan_name                VARCHAR,
                    plan_type                 VARCHAR,
                    group_number                VARCHAR,
                    payer_id                     VARCHAR,
                    cms_contract_number           VARCHAR,
                    plan_benefit_package_number    VARCHAR,
                    customer_service_phone          VARCHAR,
                    member_services_phone            VARCHAR,
                    behavioral_health_phone           VARCHAR,
                    pharmacy_help_desk_phone           VARCHAR,
                    website                              VARCHAR,
                    claims_address                        VARCHAR,
                    claim_inquiry_phone                    VARCHAR,
                    copay_pcp                               NUMERIC,
                    copay_specialist                         NUMERIC,
                    copay_er                                  NUMERIC,
                    effective_date                             DATE,
                    rx_bin                                      VARCHAR,
                    rx_pcn                                       VARCHAR,
                    rx_grp                                        VARCHAR,
                    rx_id                                          VARCHAR,
                    issued_date                                     DATE
                ) AS $$
                BEGIN
                    RETURN QUERY
                    SELECT
                        ic.id, m.id, (m.first_name || ' ' || m.last_name)::VARCHAR, m.date_of_birth,
                        e.subscriber_number, p.plan_name, p.plan_type, p.group_number, p.payer_id,
                        p.cms_contract_number, p.plan_benefit_package_number, p.customer_service_phone,
                        p.member_services_phone, p.behavioral_health_phone, p.pharmacy_help_desk_phone,
                        p.website, p.claims_address, p.claim_inquiry_phone,
                        p.copay_pcp, p.copay_specialist, p.copay_er,
                        e.effective_date, ic.rx_bin, ic.rx_pcn, ic.rx_grp, ic.rx_id, ic.issued_date
                    FROM id_cards ic
                    JOIN members m ON m.id = ic.member_id
                    JOIN enrollments e ON e.id = ic.enrollment_id
                    JOIN plans p ON p.id = e.plan_id
                    WHERE ic.member_id = p_member_id AND ic.card_status = 'active'
                    ORDER BY ic.issued_date DESC
                    LIMIT 1;
                END;
                $$ LANGUAGE plpgsql;

                -- Returns JSON (not TABLE) so the API can pass it straight to the PDF renderer.
                CREATE OR REPLACE FUNCTION fn_get_eob(p_claim_id BIGINT)
                RETURNS JSON AS $$
                DECLARE
                    result JSON;
                BEGIN
                    SELECT json_build_object(
                        'claimId', c.id,
                        'claimNumber', c.claim_number,
                        'documentNumber', c.document_number,
                        'statementDate', c.statement_date,
                        'dateReceived', c.date_received,
                        'datePaid', c.date_paid,
                        'claimStatus', c.claim_status,
                        'providerName', c.provider_name,
                        'payeeName', c.payee_name,
                        'memberId', m.id,
                        'memberName', m.first_name || ' ' || m.last_name,
                        'address', m.address_line1,
                        'city', m.city,
                        'state', m.state,
                        'zip', m.zip,
                        'subscriberNumber', e.subscriber_number,
                        'groupNumber', p.group_number,
                        'customerServicePhone', p.customer_service_phone,
                        'lines', (
                            SELECT json_agg(json_build_object(
                                'lineNo', cl.line_no,
                                'dateOfService', cl.date_of_service,
                                'serviceDescription', cl.service_description,
                                'claimStatus', cl.claim_status,
                                'providerCharges', cl.provider_charges,
                                'allowedCharges', cl.allowed_charges,
                                'copay', cl.copay,
                                'deductible', cl.deductible,
                                'coinsurance', cl.coinsurance,
                                'paidByInsurer', cl.paid_by_insurer,
                                'whatYouOwe', cl.what_you_owe,
                                'remarkCode', cl.remark_code
                            ) ORDER BY cl.line_no)
                            FROM claim_lines cl WHERE cl.claim_id = c.id
                        ),
                        'totals', (
                            SELECT json_build_object(
                                'providerCharges', COALESCE(SUM(cl.provider_charges), 0),
                                'allowedCharges', COALESCE(SUM(cl.allowed_charges), 0),
                                'copay', COALESCE(SUM(cl.copay), 0),
                                'deductible', COALESCE(SUM(cl.deductible), 0),
                                'coinsurance', COALESCE(SUM(cl.coinsurance), 0),
                                'paidByInsurer', COALESCE(SUM(cl.paid_by_insurer), 0),
                                'whatYouOwe', COALESCE(SUM(cl.what_you_owe), 0)
                            )
                            FROM claim_lines cl WHERE cl.claim_id = c.id
                        )
                    )
                    INTO result
                    FROM claims c
                    JOIN members m ON m.id = c.member_id
                    JOIN enrollments e ON e.id = c.enrollment_id
                    JOIN plans p ON p.id = e.plan_id
                    WHERE c.id = p_claim_id;

                    RETURN result;
                END;
                $$ LANGUAGE plpgsql;

                -- Term-dates any prior overlapping active enrollment on the same plan.
                CREATE OR REPLACE FUNCTION sp_enroll_member(
                    p_member_id BIGINT,
                    p_plan_id BIGINT,
                    p_subscriber_number VARCHAR,
                    p_relationship VARCHAR,
                    p_effective_date DATE,
                    p_pcp_name VARCHAR DEFAULT NULL,
                    p_pcp_phone VARCHAR DEFAULT NULL
                ) RETURNS BIGINT AS $$
                DECLARE
                    new_enrollment_id BIGINT;
                BEGIN
                    UPDATE enrollments
                    SET term_date = p_effective_date - INTERVAL '1 day', status = 'termed'
                    WHERE member_id = p_member_id
                      AND plan_id = p_plan_id
                      AND status = 'active';

                    INSERT INTO enrollments (
                        member_id, plan_id, subscriber_number, relationship,
                        effective_date, status, pcp_name, pcp_phone
                    ) VALUES (
                        p_member_id, p_plan_id, p_subscriber_number, p_relationship,
                        p_effective_date, 'active', p_pcp_name, p_pcp_phone
                    )
                    RETURNING id INTO new_enrollment_id;

                    RETURN new_enrollment_id;
                END;
                $$ LANGUAGE plpgsql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS sp_enroll_member(BIGINT, BIGINT, VARCHAR, VARCHAR, DATE, VARCHAR, VARCHAR);
                DROP FUNCTION IF EXISTS fn_get_eob(BIGINT);
                DROP FUNCTION IF EXISTS fn_get_id_card_data(BIGINT);
                DROP FUNCTION IF EXISTS fn_get_member_eligibility(BIGINT, DATE);
                """);
        }
    }
}
