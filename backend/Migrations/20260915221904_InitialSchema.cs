using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TABLE members (
                    id              BIGSERIAL PRIMARY KEY,
                    first_name      VARCHAR(100) NOT NULL,
                    last_name       VARCHAR(100) NOT NULL,
                    date_of_birth   DATE NOT NULL,
                    gender          VARCHAR(20),
                    address_line1   VARCHAR(200),
                    city            VARCHAR(100),
                    state           VARCHAR(2),
                    zip             VARCHAR(10),
                    phone           VARCHAR(20),
                    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
                );

                CREATE TABLE plans (
                    id                              BIGSERIAL PRIMARY KEY,
                    plan_name                       VARCHAR(150) NOT NULL,
                    plan_type                       VARCHAR(50) NOT NULL,
                    group_number                    VARCHAR(20) NOT NULL,
                    payer_id                        VARCHAR(20) NOT NULL,
                    cms_contract_number             VARCHAR(20),
                    plan_benefit_package_number     VARCHAR(20),
                    customer_service_phone          VARCHAR(20),
                    member_services_phone           VARCHAR(20),
                    behavioral_health_phone         VARCHAR(20),
                    pharmacy_help_desk_phone        VARCHAR(20),
                    website                         VARCHAR(200),
                    claims_address                  VARCHAR(300),
                    claim_inquiry_phone             VARCHAR(20),
                    copay_pcp                       NUMERIC(8,2) NOT NULL DEFAULT 0,
                    copay_specialist                NUMERIC(8,2) NOT NULL DEFAULT 0,
                    copay_er                        NUMERIC(8,2) NOT NULL DEFAULT 0
                );

                CREATE TABLE enrollments (
                    id                  BIGSERIAL PRIMARY KEY,
                    member_id           BIGINT NOT NULL REFERENCES members(id) ON DELETE CASCADE,
                    plan_id             BIGINT NOT NULL REFERENCES plans(id) ON DELETE RESTRICT,
                    subscriber_number   VARCHAR(30) NOT NULL,
                    relationship        VARCHAR(20) NOT NULL DEFAULT 'subscriber',
                    effective_date      DATE NOT NULL,
                    term_date           DATE,
                    status               VARCHAR(20) NOT NULL DEFAULT 'active',
                    pcp_name             VARCHAR(150),
                    pcp_phone            VARCHAR(20),
                    CONSTRAINT chk_enrollment_dates CHECK (term_date IS NULL OR term_date >= effective_date)
                );

                CREATE TABLE id_cards (
                    id              BIGSERIAL PRIMARY KEY,
                    member_id       BIGINT NOT NULL REFERENCES members(id) ON DELETE CASCADE,
                    enrollment_id   BIGINT NOT NULL REFERENCES enrollments(id) ON DELETE CASCADE,
                    rx_bin          VARCHAR(20),
                    rx_pcn          VARCHAR(20),
                    rx_grp          VARCHAR(20),
                    rx_id           VARCHAR(30),
                    issued_date     DATE NOT NULL DEFAULT CURRENT_DATE,
                    card_status     VARCHAR(20) NOT NULL DEFAULT 'active'
                );

                CREATE TABLE claims (
                    id              BIGSERIAL PRIMARY KEY,
                    member_id       BIGINT NOT NULL REFERENCES members(id) ON DELETE CASCADE,
                    enrollment_id   BIGINT NOT NULL REFERENCES enrollments(id) ON DELETE RESTRICT,
                    claim_number    VARCHAR(30) NOT NULL UNIQUE,
                    provider_name   VARCHAR(150) NOT NULL,
                    payee_name      VARCHAR(150) NOT NULL,
                    date_received   DATE NOT NULL,
                    date_paid       DATE,
                    claim_status    VARCHAR(20) NOT NULL DEFAULT 'paid',
                    statement_date  DATE NOT NULL DEFAULT CURRENT_DATE,
                    document_number VARCHAR(30) NOT NULL
                );

                CREATE TABLE claim_lines (
                    id                  BIGSERIAL PRIMARY KEY,
                    claim_id            BIGINT NOT NULL REFERENCES claims(id) ON DELETE CASCADE,
                    line_no             INT NOT NULL,
                    date_of_service      DATE NOT NULL,
                    service_description  VARCHAR(200) NOT NULL,
                    claim_status          VARCHAR(20) NOT NULL DEFAULT 'Paid',
                    provider_charges      NUMERIC(10,2) NOT NULL,
                    allowed_charges        NUMERIC(10,2) NOT NULL,
                    copay                  NUMERIC(10,2) NOT NULL DEFAULT 0,
                    deductible              NUMERIC(10,2) NOT NULL DEFAULT 0,
                    coinsurance              NUMERIC(10,2) NOT NULL DEFAULT 0,
                    paid_by_insurer           NUMERIC(10,2) NOT NULL,
                    what_you_owe               NUMERIC(10,2) NOT NULL,
                    remark_code                 VARCHAR(10),
                    UNIQUE (claim_id, line_no)
                );

                CREATE INDEX idx_enrollments_member ON enrollments(member_id);
                CREATE INDEX idx_id_cards_member ON id_cards(member_id);
                CREATE INDEX idx_claims_member ON claims(member_id);
                CREATE INDEX idx_claim_lines_claim ON claim_lines(claim_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS claim_lines;
                DROP TABLE IF EXISTS claims;
                DROP TABLE IF EXISTS id_cards;
                DROP TABLE IF EXISTS enrollments;
                DROP TABLE IF EXISTS plans;
                DROP TABLE IF EXISTS members;
                """);
        }
    }
}
