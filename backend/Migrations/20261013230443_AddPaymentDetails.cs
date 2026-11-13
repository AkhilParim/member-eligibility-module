using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    // Payment and coding detail needed for the Explanation of Payment.
    public partial class AddPaymentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE claims
                    ADD COLUMN payment_number VARCHAR(30),
                    ADD COLUMN payment_method VARCHAR(10),
                    ADD COLUMN provider_npi   VARCHAR(10);

                ALTER TABLE claim_lines
                    ADD COLUMN procedure_code         VARCHAR(10),
                    ADD COLUMN adjustment_reason_code VARCHAR(10);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE claim_lines
                    DROP COLUMN adjustment_reason_code,
                    DROP COLUMN procedure_code;

                ALTER TABLE claims
                    DROP COLUMN provider_npi,
                    DROP COLUMN payment_method,
                    DROP COLUMN payment_number;
                """);
        }
    }
}
