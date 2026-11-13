using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    // Local dev only — fabricated demo data. Gives each member several claims
    // across multiple providers (plus one denial) so member-level EOB/EOP
    // documents have something meaningful to aggregate.
    public partial class SeedAdditionalClaims : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE claims SET payment_number = 'EFT-7731045', payment_method = 'EFT',   provider_npi = '1457392016' WHERE id = 1;
                UPDATE claims SET payment_number = 'EFT-7731102', payment_method = 'EFT',   provider_npi = '1265408733' WHERE id = 2;
                UPDATE claims SET payment_number = 'CHK-0048170', payment_method = 'CHECK', provider_npi = '1093847562' WHERE id = 3;

                UPDATE claim_lines SET adjustment_reason_code = 'CO-45' WHERE provider_charges > allowed_charges;
                UPDATE claim_lines SET procedure_code = '99213' WHERE claim_id = 2 AND line_no = 1;
                UPDATE claim_lines SET procedure_code = '93000' WHERE claim_id = 2 AND line_no = 2;
                UPDATE claim_lines SET procedure_code = '99203' WHERE claim_id = 3 AND line_no = 1;
                UPDATE claim_lines SET procedure_code = '73110' WHERE claim_id = 3 AND line_no = 2;

                INSERT INTO claims (id, member_id, enrollment_id, claim_number, provider_name, payee_name, date_received, date_paid,
                                    claim_status, statement_date, document_number, payment_number, payment_method, provider_npi) VALUES
                    (4,  1, 1, 'CLM00098768', 'Springfield Family Medicine',    'Springfield Family Medicine',    '2025-05-12', '2025-05-20', 'paid',   '2025-05-22', 'DOC0004455667', 'EFT-7740021', 'EFT',   '1457392016'),
                    (5,  1, 1, 'CLM00098769', 'Northside Imaging Center',       'Northside Imaging Center',       '2025-07-14', '2025-07-24', 'paid',   '2025-07-26', 'DOC0005566778', 'CHK-0048391', 'CHECK', '1730259418'),
                    (6,  1, 1, 'CLM00098770', 'Springfield Family Medicine',    'Springfield Family Medicine',    '2025-09-03', '2025-09-10', 'denied', '2025-09-12', 'DOC0006677889', NULL,          NULL,    '1457392016'),
                    (7,  2, 2, 'CLM00098771', 'Lakeside Cardiology Associates', 'Lakeside Cardiology Associates', '2025-04-18', '2025-04-28', 'paid',   '2025-04-30', 'DOC0007788990', 'EFT-7740188', 'EFT',   '1265408733'),
                    (8,  2, 2, 'CLM00098772', 'Central Lab Services',           'Central Lab Services',           '2025-06-02', '2025-06-12', 'paid',   '2025-06-14', 'DOC0008899001', 'CHK-0048522', 'CHECK', '1558036294'),
                    (9,  3, 3, 'CLM00098773', 'Riverbend Family Practice',      'Riverbend Family Practice',      '2025-02-18', '2025-02-27', 'paid',   '2025-03-01', 'DOC0009900112', 'EFT-7740305', 'EFT',   '1881203947'),
                    (10, 3, 3, 'CLM00098774', 'Metro Orthopedics',              'Metro Orthopedics',              '2025-05-06', '2025-05-16', 'paid',   '2025-05-18', 'DOC0010011223', 'EFT-7740377', 'EFT',   '1649315827');
                SELECT setval('claims_id_seq', 10);

                INSERT INTO claim_lines (claim_id, line_no, date_of_service, service_description, claim_status,
                                         provider_charges, allowed_charges, copay, deductible, coinsurance,
                                         paid_by_insurer, what_you_owe, remark_code, procedure_code, adjustment_reason_code) VALUES
                    (4,  1, '2025-05-08', 'Office visit - established patient', 'Paid',    180.00,  95.00,  0.00,  0.00, 0.00,  95.00,  0.00, NULL, '99213', 'CO-45'),
                    (4,  2, '2025-05-08', 'Comprehensive metabolic panel',      'Paid',     65.00,  14.50,  0.00,  0.00, 0.00,  14.50,  0.00, NULL, '80053', 'CO-45'),
                    (5,  1, '2025-07-10', 'MRI knee without contrast',          'Paid',   1450.00, 480.00,  0.00,  0.00, 0.00, 480.00,  0.00, NULL, '73721', 'CO-45'),
                    (6,  1, '2025-08-29', 'Chiropractic manipulation',          'Denied',   95.00,   0.00,  0.00,  0.00, 0.00,   0.00,  0.00, NULL, '98940', 'CO-96'),
                    (7,  1, '2025-04-15', 'Echocardiogram, complete',           'Paid',    820.00, 310.00,  0.00,  0.00, 0.00, 310.00,  0.00, NULL, '93306', 'CO-45'),
                    (7,  2, '2025-04-15', 'Office visit - established patient', 'Paid',    260.00, 120.00,  0.00,  0.00, 0.00, 120.00,  0.00, NULL, '99214', 'CO-45'),
                    (8,  1, '2025-05-30', 'Lipid panel',                        'Paid',    120.00,  22.00,  0.00,  0.00, 0.00,  22.00,  0.00, NULL, '80061', 'CO-45'),
                    (8,  2, '2025-05-30', 'Hemoglobin A1c',                     'Paid',     80.00,  13.00,  0.00,  0.00, 0.00,  13.00,  0.00, NULL, '83036', 'CO-45'),
                    (9,  1, '2025-02-14', 'Office visit - new patient',         'Paid',    240.00, 150.00, 20.00,  0.00, 0.00, 130.00, 20.00, NULL, '99203', 'CO-45'),
                    (9,  2, '2025-02-14', 'Influenza vaccine',                  'Paid',     45.00,  30.00,  0.00,  0.00, 0.00,  30.00,  0.00, NULL, '90686', 'CO-45'),
                    (10, 1, '2025-05-02', 'Specialist office visit',            'Paid',    310.00, 200.00, 40.00,  0.00, 0.00, 160.00, 40.00, NULL, '99214', 'CO-45'),
                    (10, 2, '2025-05-02', 'Joint injection, major joint',       'Paid',    180.00,  90.00,  0.00, 50.00, 8.00,  32.00, 58.00, NULL, '20610', 'CO-45');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM claim_lines WHERE claim_id BETWEEN 4 AND 10;
                DELETE FROM claims WHERE id BETWEEN 4 AND 10;

                UPDATE claim_lines SET procedure_code = NULL, adjustment_reason_code = NULL WHERE claim_id IN (1, 2, 3);
                UPDATE claims SET payment_number = NULL, payment_method = NULL, provider_npi = NULL WHERE id IN (1, 2, 3);
                """);
        }
    }
}
