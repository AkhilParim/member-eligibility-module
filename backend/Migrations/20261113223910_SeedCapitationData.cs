using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    // Local dev only: fabricated demo data (no real PII, MBIs, or provider IDs).
    // Brings the member count to 10 and gives the MMR cases the capitation calculation has to handle:
    //  - Patricia (5) joins in Aug, so she has no Jul row
    //  - Harold (8) is enrolled retroactively: Aug has a regular row plus a +1 adjustment for July
    //  - William (6) is disenrolled retroactively: Sep has only a -1 adjustment reversing Aug
    // Adjustment codes RD / RE are placeholders; the real set is in the CMS Plan Communications User Guide.
    public partial class SeedCapitationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE plans SET cms_contract_number = 'H5678', plan_benefit_package_number = '001' WHERE id = 2;

                INSERT INTO provider_contracts (id, contract_number, provider_group_name, provider_tin, pmpm_rate, effective_date, term_date) VALUES
                    (1, 'PC-1001', 'Springfield Primary Care IPA', '36-1234567', 725.00, '2025-01-01', NULL),
                    (2, 'PC-1002', 'Lakeside Medical Group',       '36-2345678', 790.00, '2025-01-01', NULL),
                    (3, 'PC-1003', 'Riverbend Health Partners',    '36-3456789', 842.50, '2025-01-01', NULL);
                SELECT setval('provider_contracts_id_seq', 3);

                INSERT INTO members (id, first_name, last_name, date_of_birth, gender, address_line1, city, state, zip, phone, mbi, provider_contract_id) VALUES
                    (4,  'Robert',   'Thompson', '1951-02-09', 'M', '1530 Elm St',        'Springfield', 'IL', '62704', '217-555-0144', '4EF5GH6JK78', 1),
                    (5,  'Patricia', 'Alvarez',  '1957-06-30', 'F', '88 Birch Ln',        'Springfield', 'IL', '62704', '217-555-0155', '5FG6HJ7KM89', 1),
                    (6,  'William',  'Chen',     '1946-12-14', 'M', '301 Cedar Ct',       'Springfield', 'IL', '62705', '217-555-0166', '6GH7JK8MN91', 1),
                    (7,  'Dorothy',  'Williams', '1944-09-03', 'F', '77 Prairie Ave',     'Springfield', 'IL', '62705', '217-555-0177', '7HJ8KM9NP12', 2),
                    (8,  'Harold',   'Brooks',   '1953-04-21', 'M', '945 River Rd',       'Springfield', 'IL', '62706', '217-555-0188', '8JK9MN1PQ23', 2),
                    (9,  'Evelyn',   'Patel',    '1959-08-17', 'F', '12 Lakeview Dr',     'Springfield', 'IL', '62706', '217-555-0199', '9KM1NP2QR34', 3),
                    (10, 'Frank',    'Delgado',  '1949-01-28', 'M', '460 Sycamore Blvd',  'Springfield', 'IL', '62707', '217-555-0210', '1MN2PQ3RT45', 3);
                SELECT setval('members_id_seq', 10);

                UPDATE members SET mbi = '1AC2DE3FG45', provider_contract_id = 1 WHERE id = 1;
                UPDATE members SET mbi = '2CJ3KM4NP56', provider_contract_id = 2 WHERE id = 2;
                UPDATE members SET mbi = '3DE4FG5HJ67', provider_contract_id = 3 WHERE id = 3;

                INSERT INTO enrollments (id, member_id, plan_id, subscriber_number, relationship, effective_date, term_date, status, pcp_name, pcp_phone) VALUES
                    (4,  4,  1, 'SUB100234504', 'subscriber', '2024-06-01', NULL,         'active', 'Dr. Alan Kim',     '217-555-0301'),
                    (5,  5,  1, 'SUB100234505', 'subscriber', '2025-08-01', NULL,         'active', 'Dr. Alan Kim',     '217-555-0301'),
                    (6,  6,  2, 'SUB100234506', 'subscriber', '2023-11-01', '2025-07-31', 'termed', 'Dr. Omar Farouk',  '217-555-0303'),
                    (7,  7,  1, 'SUB100234507', 'subscriber', '2024-01-01', NULL,         'active', 'Dr. Susan Lee',    '217-555-0302'),
                    (8,  8,  2, 'SUB100234508', 'subscriber', '2025-07-01', NULL,         'active', 'Dr. Susan Lee',    '217-555-0302'),
                    (9,  9,  2, 'SUB100234509', 'subscriber', '2024-03-01', NULL,         'active', 'Dr. Omar Farouk',  '217-555-0303'),
                    (10, 10, 1, 'SUB100234510', 'subscriber', '2024-09-01', NULL,         'active', 'Dr. Priya Nair',   '217-555-0304');
                SELECT setval('enrollments_id_seq', 10);

                INSERT INTO id_cards (id, member_id, enrollment_id, rx_bin, rx_pcn, rx_grp, rx_id, issued_date, card_status) VALUES
                    (4,  4,  4,  '610014', 'PLAN01', 'RXGRP01', 'SUB100234504', '2024-06-01', 'active'),
                    (5,  5,  5,  '610014', 'PLAN01', 'RXGRP01', 'SUB100234505', '2025-08-01', 'active'),
                    (6,  6,  6,  '610591', 'PPO02',  'RXGRP02', 'SUB100234506', '2023-11-01', 'cancelled'),
                    (7,  7,  7,  '610014', 'PLAN01', 'RXGRP01', 'SUB100234507', '2024-01-01', 'active'),
                    (8,  8,  8,  '610591', 'PPO02',  'RXGRP02', 'SUB100234508', '2025-07-01', 'active'),
                    (9,  9,  9,  '610591', 'PPO02',  'RXGRP02', 'SUB100234509', '2024-03-01', 'active'),
                    (10, 10, 10, '610014', 'PLAN01', 'RXGRP01', 'SUB100234510', '2024-09-01', 'active');
                SELECT setval('id_cards_id_seq', 10);

                INSERT INTO mmr_records (contract_number, plan_benefit_package, beneficiary_id, payment_month, adjustment_reason_code,
                                         member_months, part_c_risk_factor, part_a_payment, part_b_payment, part_d_payment) VALUES
                    ('H1234', '001', '1AC2DE3FG45', '2025-07-01', NULL,  1, 1.2450,   498.00,   771.90,  118.25),
                    ('H1234', '001', '2CJ3KM4NP56', '2025-07-01', NULL,  1, 1.6120,   644.80,   999.44,  131.40),
                    ('H5678', '001', '3DE4FG5HJ67', '2025-07-01', NULL,  1, 0.9210,   368.40,   571.02,   96.10),
                    ('H1234', '001', '4EF5GH6JK78', '2025-07-01', NULL,  1, 1.0380,   415.20,   643.56,  102.35),
                    ('H5678', '001', '6GH7JK8MN91', '2025-07-01', NULL,  1, 2.0840,   833.60,  1292.08,  142.75),
                    ('H1234', '001', '7HJ8KM9NP12', '2025-07-01', NULL,  1, 1.4390,   575.60,   892.18,  125.60),
                    ('H5678', '001', '9KM1NP2QR34', '2025-07-01', NULL,  1, 0.9760,   390.40,   605.12,   99.80),
                    ('H1234', '001', '1MN2PQ3RT45', '2025-07-01', NULL,  1, 1.3010,   520.40,   806.62,  121.15),
                    ('H1234', '001', '1AC2DE3FG45', '2025-08-01', NULL,  1, 1.2450,   498.00,   771.90,  118.25),
                    ('H1234', '001', '2CJ3KM4NP56', '2025-08-01', NULL,  1, 1.6120,   644.80,   999.44,  131.40),
                    ('H5678', '001', '3DE4FG5HJ67', '2025-08-01', NULL,  1, 0.9210,   368.40,   571.02,   96.10),
                    ('H1234', '001', '4EF5GH6JK78', '2025-08-01', NULL,  1, 1.0380,   415.20,   643.56,  102.35),
                    ('H1234', '001', '5FG6HJ7KM89', '2025-08-01', NULL,  1, 0.8470,   338.80,   525.14,   88.90),
                    ('H5678', '001', '6GH7JK8MN91', '2025-08-01', NULL,  1, 2.0840,   833.60,  1292.08,  142.75),
                    ('H1234', '001', '7HJ8KM9NP12', '2025-08-01', NULL,  1, 1.4390,   575.60,   892.18,  125.60),
                    ('H5678', '001', '8JK9MN1PQ23', '2025-08-01', NULL,  1, 1.1020,   440.80,   683.24,  109.30),
                    ('H5678', '001', '9KM1NP2QR34', '2025-08-01', NULL,  1, 0.9760,   390.40,   605.12,   99.80),
                    ('H1234', '001', '1MN2PQ3RT45', '2025-08-01', NULL,  1, 1.3010,   520.40,   806.62,  121.15),
                    ('H5678', '001', '8JK9MN1PQ23', '2025-08-01', 'RE',  1, 1.1020,   440.80,   683.24,  109.30),
                    ('H1234', '001', '1AC2DE3FG45', '2025-09-01', NULL,  1, 1.2450,   498.00,   771.90,  118.25),
                    ('H1234', '001', '2CJ3KM4NP56', '2025-09-01', NULL,  1, 1.6120,   644.80,   999.44,  131.40),
                    ('H5678', '001', '3DE4FG5HJ67', '2025-09-01', NULL,  1, 0.9210,   368.40,   571.02,   96.10),
                    ('H1234', '001', '4EF5GH6JK78', '2025-09-01', NULL,  1, 1.0380,   415.20,   643.56,  102.35),
                    ('H1234', '001', '5FG6HJ7KM89', '2025-09-01', NULL,  1, 0.8470,   338.80,   525.14,   88.90),
                    ('H1234', '001', '7HJ8KM9NP12', '2025-09-01', NULL,  1, 1.4390,   575.60,   892.18,  125.60),
                    ('H5678', '001', '8JK9MN1PQ23', '2025-09-01', NULL,  1, 1.1020,   440.80,   683.24,  109.30),
                    ('H5678', '001', '9KM1NP2QR34', '2025-09-01', NULL,  1, 0.9760,   390.40,   605.12,   99.80),
                    ('H1234', '001', '1MN2PQ3RT45', '2025-09-01', NULL,  1, 1.3010,   520.40,   806.62,  121.15),
                    ('H5678', '001', '6GH7JK8MN91', '2025-09-01', 'RD', -1, 2.0840,  -833.60, -1292.08, -142.75);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM mmr_records;

                DELETE FROM members WHERE id BETWEEN 4 AND 10;
                UPDATE members SET mbi = NULL, provider_contract_id = NULL WHERE id IN (1, 2, 3);
                DELETE FROM provider_contracts WHERE id IN (1, 2, 3);

                UPDATE plans SET cms_contract_number = NULL, plan_benefit_package_number = NULL WHERE id = 2;
                """);
        }
    }
}
