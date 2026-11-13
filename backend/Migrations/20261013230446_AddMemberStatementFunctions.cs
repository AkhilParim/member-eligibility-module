using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    public partial class AddMemberStatementFunctions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                -- Member-level EOB: every claim for the member, with lines and totals.
                -- p_from/p_to filter on line date of service; claims with no in-range lines are omitted.
                CREATE OR REPLACE FUNCTION fn_get_member_eob(
                    p_member_id BIGINT,
                    p_from DATE DEFAULT NULL,
                    p_to DATE DEFAULT NULL
                ) RETURNS JSON AS $$
                DECLARE
                    result JSON;
                BEGIN
                    WITH member_header AS (
                        SELECT m.id, m.first_name, m.last_name, m.address_line1, m.city, m.state, m.zip,
                               e.subscriber_number, p.group_number, p.plan_name, p.customer_service_phone
                        FROM members m
                        LEFT JOIN LATERAL (
                            SELECT * FROM enrollments WHERE member_id = m.id ORDER BY effective_date DESC LIMIT 1
                        ) e ON TRUE
                        LEFT JOIN plans p ON p.id = e.plan_id
                        WHERE m.id = p_member_id
                    ),
                    in_range AS (
                        SELECT cl.*
                        FROM claim_lines cl
                        JOIN claims c ON c.id = cl.claim_id
                        WHERE c.member_id = p_member_id
                          AND (p_from IS NULL OR cl.date_of_service >= p_from)
                          AND (p_to   IS NULL OR cl.date_of_service <= p_to)
                    )
                    SELECT json_build_object(
                        'memberId', h.id,
                        'memberName', h.first_name || ' ' || h.last_name,
                        'address', h.address_line1,
                        'city', h.city,
                        'state', h.state,
                        'zip', h.zip,
                        'subscriberNumber', h.subscriber_number,
                        'groupNumber', h.group_number,
                        'planName', h.plan_name,
                        'customerServicePhone', h.customer_service_phone,
                        'statementDate', CURRENT_DATE,
                        'periodFrom', p_from,
                        'periodTo', p_to,
                        'claims', COALESCE((
                            SELECT json_agg(json_build_object(
                                'claimId', c.id,
                                'claimNumber', c.claim_number,
                                'documentNumber', c.document_number,
                                'statementDate', c.statement_date,
                                'dateReceived', c.date_received,
                                'datePaid', c.date_paid,
                                'claimStatus', c.claim_status,
                                'providerName', c.provider_name,
                                'payeeName', c.payee_name,
                                'lines', (
                                    SELECT json_agg(json_build_object(
                                        'lineNo', l.line_no,
                                        'dateOfService', l.date_of_service,
                                        'serviceDescription', l.service_description,
                                        'claimStatus', l.claim_status,
                                        'providerCharges', l.provider_charges,
                                        'allowedCharges', l.allowed_charges,
                                        'copay', l.copay,
                                        'deductible', l.deductible,
                                        'coinsurance', l.coinsurance,
                                        'paidByInsurer', l.paid_by_insurer,
                                        'whatYouOwe', l.what_you_owe,
                                        'remarkCode', l.remark_code
                                    ) ORDER BY l.line_no)
                                    FROM in_range l WHERE l.claim_id = c.id
                                ),
                                'totals', (
                                    SELECT json_build_object(
                                        'providerCharges', COALESCE(SUM(l.provider_charges), 0),
                                        'allowedCharges', COALESCE(SUM(l.allowed_charges), 0),
                                        'copay', COALESCE(SUM(l.copay), 0),
                                        'deductible', COALESCE(SUM(l.deductible), 0),
                                        'coinsurance', COALESCE(SUM(l.coinsurance), 0),
                                        'paidByInsurer', COALESCE(SUM(l.paid_by_insurer), 0),
                                        'whatYouOwe', COALESCE(SUM(l.what_you_owe), 0)
                                    )
                                    FROM in_range l WHERE l.claim_id = c.id
                                )
                            ) ORDER BY c.date_received, c.id)
                            FROM claims c
                            WHERE c.member_id = p_member_id
                              AND EXISTS (SELECT 1 FROM in_range l WHERE l.claim_id = c.id)
                        ), '[]'::json),
                        'totals', (
                            SELECT json_build_object(
                                'providerCharges', COALESCE(SUM(provider_charges), 0),
                                'allowedCharges', COALESCE(SUM(allowed_charges), 0),
                                'copay', COALESCE(SUM(copay), 0),
                                'deductible', COALESCE(SUM(deductible), 0),
                                'coinsurance', COALESCE(SUM(coinsurance), 0),
                                'paidByInsurer', COALESCE(SUM(paid_by_insurer), 0),
                                'whatYouOwe', COALESCE(SUM(what_you_owe), 0)
                            )
                            FROM in_range
                        )
                    )
                    INTO result
                    FROM member_header h;

                    RETURN result;
                END;
                $$ LANGUAGE plpgsql;

                -- Explanation of Payment: the provider-facing remittance for a member's processed claims,
                -- grouped by payment (payee + payment number). Pending claims are excluded.
                -- Layout follows the CMS Standard Paper Remittance Advice.
                CREATE OR REPLACE FUNCTION fn_get_member_eop(
                    p_member_id BIGINT,
                    p_from DATE DEFAULT NULL,
                    p_to DATE DEFAULT NULL
                ) RETURNS JSON AS $$
                DECLARE
                    result JSON;
                BEGIN
                    WITH member_header AS (
                        SELECT m.id, m.first_name, m.last_name,
                               e.subscriber_number, p.group_number, p.plan_name, p.payer_id,
                               p.claim_inquiry_phone, p.claims_address
                        FROM members m
                        LEFT JOIN LATERAL (
                            SELECT * FROM enrollments WHERE member_id = m.id ORDER BY effective_date DESC LIMIT 1
                        ) e ON TRUE
                        LEFT JOIN plans p ON p.id = e.plan_id
                        WHERE m.id = p_member_id
                    ),
                    in_range AS (
                        SELECT cl.*
                        FROM claim_lines cl
                        JOIN claims c ON c.id = cl.claim_id
                        WHERE c.member_id = p_member_id
                          AND c.claim_status <> 'pending'
                          AND (p_from IS NULL OR cl.date_of_service >= p_from)
                          AND (p_to   IS NULL OR cl.date_of_service <= p_to)
                    ),
                    claim_totals AS (
                        SELECT claim_id,
                               SUM(provider_charges)                    AS billed,
                               SUM(provider_charges - allowed_charges)  AS adjustment,
                               SUM(allowed_charges)                     AS allowed,
                               SUM(copay)                               AS copay,
                               SUM(deductible)                          AS deductible,
                               SUM(coinsurance)                         AS coinsurance,
                               SUM(what_you_owe)                        AS patient_responsibility,
                               SUM(paid_by_insurer)                     AS paid
                        FROM in_range
                        GROUP BY claim_id
                    ),
                    claim_docs AS (
                        SELECT c.id, c.payee_name, c.provider_npi, c.payment_number, c.payment_method, c.date_paid,
                               t.billed, t.adjustment, t.allowed, t.copay, t.deductible, t.coinsurance,
                               t.patient_responsibility, t.paid,
                               json_build_object(
                                   'claimId', c.id,
                                   'claimNumber', c.claim_number,
                                   'documentNumber', c.document_number,
                                   'providerName', c.provider_name,
                                   'claimStatus', c.claim_status,
                                   'dateReceived', c.date_received,
                                   'lines', (
                                       SELECT json_agg(json_build_object(
                                           'lineNo', l.line_no,
                                           'dateOfService', l.date_of_service,
                                           'procedureCode', l.procedure_code,
                                           'serviceDescription', l.service_description,
                                           'lineStatus', l.claim_status,
                                           'billed', l.provider_charges,
                                           'contractualAdjustment', l.provider_charges - l.allowed_charges,
                                           'allowed', l.allowed_charges,
                                           'copay', l.copay,
                                           'deductible', l.deductible,
                                           'coinsurance', l.coinsurance,
                                           'patientResponsibility', l.what_you_owe,
                                           'paid', l.paid_by_insurer,
                                           'reasonCode', l.adjustment_reason_code,
                                           'remarkCode', l.remark_code
                                       ) ORDER BY l.line_no)
                                       FROM in_range l WHERE l.claim_id = c.id
                                   ),
                                   'totals', json_build_object(
                                       'billed', t.billed,
                                       'contractualAdjustment', t.adjustment,
                                       'allowed', t.allowed,
                                       'copay', t.copay,
                                       'deductible', t.deductible,
                                       'coinsurance', t.coinsurance,
                                       'patientResponsibility', t.patient_responsibility,
                                       'paid', t.paid
                                   )
                               ) AS doc
                        FROM claims c
                        JOIN claim_totals t ON t.claim_id = c.id
                        WHERE c.member_id = p_member_id
                          AND c.claim_status <> 'pending'
                    ),
                    payment_groups AS (
                        SELECT payee_name, provider_npi, payment_number, payment_method, date_paid,
                               json_agg(doc ORDER BY id)        AS claims,
                               COUNT(*)                         AS claim_count,
                               SUM(billed)                      AS billed,
                               SUM(adjustment)                  AS adjustment,
                               SUM(allowed)                     AS allowed,
                               SUM(copay)                       AS copay,
                               SUM(deductible)                  AS deductible,
                               SUM(coinsurance)                 AS coinsurance,
                               SUM(patient_responsibility)      AS patient_responsibility,
                               SUM(paid)                        AS paid
                        FROM claim_docs
                        GROUP BY payee_name, provider_npi, payment_number, payment_method, date_paid
                    )
                    SELECT json_build_object(
                        'memberId', h.id,
                        'memberName', h.first_name || ' ' || h.last_name,
                        'subscriberNumber', h.subscriber_number,
                        'groupNumber', h.group_number,
                        'payerName', h.plan_name,
                        'payerId', h.payer_id,
                        'payerPhone', h.claim_inquiry_phone,
                        'payerAddress', h.claims_address,
                        'statementDate', CURRENT_DATE,
                        'periodFrom', p_from,
                        'periodTo', p_to,
                        'claimCount', (SELECT COUNT(*) FROM claim_docs),
                        'payments', COALESCE((
                            SELECT json_agg(json_build_object(
                                'payeeName', g.payee_name,
                                'providerNpi', g.provider_npi,
                                'paymentNumber', g.payment_number,
                                'paymentMethod', g.payment_method,
                                'paymentDate', g.date_paid,
                                'claimCount', g.claim_count,
                                'claims', g.claims,
                                'totals', json_build_object(
                                    'billed', g.billed,
                                    'contractualAdjustment', g.adjustment,
                                    'allowed', g.allowed,
                                    'copay', g.copay,
                                    'deductible', g.deductible,
                                    'coinsurance', g.coinsurance,
                                    'patientResponsibility', g.patient_responsibility,
                                    'paid', g.paid
                                )
                            ) ORDER BY g.payee_name, g.date_paid NULLS LAST, g.payment_number)
                            FROM payment_groups g
                        ), '[]'::json),
                        'totals', (
                            SELECT json_build_object(
                                'billed', COALESCE(SUM(billed), 0),
                                'contractualAdjustment', COALESCE(SUM(adjustment), 0),
                                'allowed', COALESCE(SUM(allowed), 0),
                                'copay', COALESCE(SUM(copay), 0),
                                'deductible', COALESCE(SUM(deductible), 0),
                                'coinsurance', COALESCE(SUM(coinsurance), 0),
                                'patientResponsibility', COALESCE(SUM(patient_responsibility), 0),
                                'paid', COALESCE(SUM(paid), 0)
                            )
                            FROM claim_docs
                        )
                    )
                    INTO result
                    FROM member_header h;

                    RETURN result;
                END;
                $$ LANGUAGE plpgsql;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP FUNCTION IF EXISTS fn_get_member_eop(BIGINT, DATE, DATE);
                DROP FUNCTION IF EXISTS fn_get_member_eob(BIGINT, DATE, DATE);
                """);
        }
    }
}
