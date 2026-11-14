using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    // Provider contracts (the PMPM rate) and a subset of the CMS Monthly Membership Report,
    // plus the MBI and contract assignment on members that tie the two together.
    public partial class AddCapitationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE provider_contracts (
                    id                  BIGSERIAL PRIMARY KEY,
                    contract_number     VARCHAR(20) NOT NULL UNIQUE,
                    provider_group_name VARCHAR(150) NOT NULL,
                    provider_tin        VARCHAR(10),
                    pmpm_rate           NUMERIC(10,2) NOT NULL CHECK (pmpm_rate > 0),
                    effective_date      DATE NOT NULL,
                    term_date           DATE,
                    CONSTRAINT chk_provider_contract_dates CHECK (term_date IS NULL OR term_date >= effective_date)
                );

                CREATE TABLE mmr_records (
                    id                      BIGSERIAL PRIMARY KEY,
                    contract_number         VARCHAR(5) NOT NULL,
                    plan_benefit_package    VARCHAR(3) NOT NULL,
                    beneficiary_id          VARCHAR(11) NOT NULL,
                    payment_month           DATE NOT NULL,
                    adjustment_reason_code  VARCHAR(2),
                    member_months           INT NOT NULL DEFAULT 1,
                    part_c_risk_factor      NUMERIC(7,4) NOT NULL,
                    part_a_payment          NUMERIC(10,2) NOT NULL DEFAULT 0,
                    part_b_payment          NUMERIC(10,2) NOT NULL DEFAULT 0,
                    part_d_payment          NUMERIC(10,2) NOT NULL DEFAULT 0,
                    CONSTRAINT chk_mmr_payment_month_first_day CHECK (EXTRACT(DAY FROM payment_month) = 1)
                );

                CREATE INDEX idx_mmr_payment_month ON mmr_records(payment_month);
                CREATE INDEX idx_mmr_beneficiary ON mmr_records(beneficiary_id);

                ALTER TABLE members
                    ADD COLUMN mbi VARCHAR(11) UNIQUE,
                    ADD COLUMN provider_contract_id BIGINT REFERENCES provider_contracts(id) ON DELETE SET NULL;

                CREATE INDEX idx_members_provider_contract ON members(provider_contract_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS idx_members_provider_contract;

                ALTER TABLE members
                    DROP COLUMN provider_contract_id,
                    DROP COLUMN mbi;

                DROP TABLE IF EXISTS mmr_records;
                DROP TABLE IF EXISTS provider_contracts;
                """);
        }
    }
}
