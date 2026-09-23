using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MemberEligibility.Api.Migrations
{
    // Fixes rows already seeded by SeedData before an earlier placeholder
    // plan name/group number/website were replaced. No-op on a fresh
    // database, since SeedData now inserts the corrected values directly.
    public partial class FixSeedPlanNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE plans SET plan_name = 'Meridian Health Advantage Dual (HMO D-SNP)', group_number = 'MERID01', website = 'www.meridianhealthplan.com'
                WHERE plan_name = 'Ordra Health Advantage Dual (HMO D-SNP)';

                UPDATE plans SET plan_name = 'Meridian Health PPO Select', group_number = 'MERID02', website = 'www.meridianhealthplan.com'
                WHERE plan_name = 'Ordra Health PPO Select';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE plans SET plan_name = 'Ordra Health Advantage Dual (HMO D-SNP)', group_number = 'ORDRA01', website = 'www.ordrahealthplan.com'
                WHERE plan_name = 'Meridian Health Advantage Dual (HMO D-SNP)';

                UPDATE plans SET plan_name = 'Ordra Health PPO Select', group_number = 'ORDRA02', website = 'www.ordrahealthplan.com'
                WHERE plan_name = 'Meridian Health PPO Select';
                """);
        }
    }
}
