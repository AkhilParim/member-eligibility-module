using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    // Local dev only — do not run against a real production database.
    // CLM00098765's line items match the CMS sample EOB (Pub #11819) exactly.
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO members (id, first_name, last_name, date_of_birth, gender, address_line1, city, state, zip, phone) VALUES
                    (1, 'Maria',  'Gonzalez', '1955-03-12', 'F', '412 Willow St',   'Springfield', 'IL', '62701', '217-555-0111'),
                    (2, 'James',  'Carter',   '1948-11-02', 'M', '87 Oak Ave',      'Springfield', 'IL', '62702', '217-555-0122'),
                    (3, 'Linda',  'Nguyen',   '1962-07-25', 'F', '2290 Maple Dr',   'Springfield', 'IL', '62703', '217-555-0133');
                SELECT setval('members_id_seq', 3);

                INSERT INTO plans (id, plan_name, plan_type, group_number, payer_id, cms_contract_number, plan_benefit_package_number,
                                    customer_service_phone, member_services_phone, behavioral_health_phone, pharmacy_help_desk_phone,
                                    website, claims_address, claim_inquiry_phone, copay_pcp, copay_specialist, copay_er) VALUES
                    (1, 'Meridian Health Advantage Dual (HMO D-SNP)', 'Dual-Eligible SNP', 'MERID01', '60054', 'H1234', '001',
                     '1-800-555-0100', '1-800-555-0101', '1-800-555-0102', '1-800-555-0103',
                     'www.meridianhealthplan.com', 'PO Box 1000, Springfield, IL 62701', '1-800-555-0104', 0, 0, 0),
                    (2, 'Meridian Health PPO Select', 'PPO', 'MERID02', '60055', NULL, NULL,
                     '1-800-555-0200', '1-800-555-0201', '1-800-555-0202', '1-800-555-0203',
                     'www.meridianhealthplan.com', 'PO Box 2000, Springfield, IL 62701', '1-800-555-0204', 20, 40, 100);
                SELECT setval('plans_id_seq', 2);

                INSERT INTO enrollments (id, member_id, plan_id, subscriber_number, relationship, effective_date, term_date, status, pcp_name, pcp_phone) VALUES
                    (1, 1, 1, 'SUB100234501', 'subscriber', '2024-01-01', NULL, 'active', 'Dr. Alan Kim',    '217-555-0301'),
                    (2, 2, 1, 'SUB100234502', 'subscriber', '2023-06-01', NULL, 'active', 'Dr. Susan Lee',   '217-555-0302'),
                    (3, 3, 2, 'SUB100234503', 'subscriber', '2025-01-01', NULL, 'active', 'Dr. Omar Farouk', '217-555-0303');
                SELECT setval('enrollments_id_seq', 3);

                INSERT INTO id_cards (id, member_id, enrollment_id, rx_bin, rx_pcn, rx_grp, rx_id, issued_date, card_status) VALUES
                    (1, 1, 1, '610014', 'PLAN01', 'RXGRP01', 'SUB100234501', '2024-01-01', 'active'),
                    (2, 2, 2, '610014', 'PLAN01', 'RXGRP01', 'SUB100234502', '2023-06-01', 'active'),
                    (3, 3, 3, '610591', 'PPO02',  'RXGRP02', 'SUB100234503', '2025-01-01', 'active');
                SELECT setval('id_cards_id_seq', 3);

                INSERT INTO claims (id, member_id, enrollment_id, claim_number, provider_name, payee_name, date_received, date_paid, claim_status, statement_date, document_number) VALUES
                    (1, 1, 1, 'CLM00098765', 'Springfield Family Medicine',    'Springfield Family Medicine',    '2025-03-22', '2025-04-02', 'paid', '2025-04-05', 'DOC0001122334'),
                    (2, 2, 2, 'CLM00098766', 'Lakeside Cardiology Associates', 'Lakeside Cardiology Associates', '2025-02-10', '2025-02-20', 'paid', '2025-02-22', 'DOC0002233445'),
                    (3, 3, 3, 'CLM00098767', 'Metro Urgent Care',               'Metro Urgent Care',               '2025-08-01', '2025-08-10', 'paid', '2025-08-12', 'DOC0003344556');
                SELECT setval('claims_id_seq', 3);

                INSERT INTO claim_lines (claim_id, line_no, date_of_service, service_description, claim_status,
                                          provider_charges, allowed_charges, copay, deductible, coinsurance, paid_by_insurer, what_you_owe, remark_code) VALUES
                    (1, 1, '2025-03-20', 'Medical care', 'Paid',  31.60,   2.15,  0.00,  0.00, 0.00,   2.15,  0.00, 'PDC'),
                    (1, 2, '2025-03-20', 'Medical care', 'Paid', 375.00, 118.12, 35.00,  0.00, 0.00,  83.12, 35.00, 'PDC'),
                    (2, 1, '2025-02-05', 'Office visit - established patient', 'Paid', 210.00, 150.00, 0.00, 0.00, 0.00, 150.00, 0.00, NULL),
                    (2, 2, '2025-02-05', 'EKG - routine',                       'Paid',  95.00,  60.00, 0.00, 0.00, 0.00,  60.00, 0.00, NULL),
                    (3, 1, '2025-07-28', 'Urgent care visit', 'Paid', 180.00, 140.00, 40.00,  0.00,  0.00, 100.00, 40.00, NULL),
                    (3, 2, '2025-07-28', 'X-ray, wrist',       'Paid', 220.00, 160.00,  0.00, 50.00, 22.00,  88.00, 72.00, NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM claim_lines WHERE claim_id IN (1, 2, 3);
                DELETE FROM claims WHERE id IN (1, 2, 3);
                DELETE FROM id_cards WHERE id IN (1, 2, 3);
                DELETE FROM enrollments WHERE id IN (1, 2, 3);
                DELETE FROM plans WHERE id IN (1, 2);
                DELETE FROM members WHERE id IN (1, 2, 3);
                """);
        }
    }
}
