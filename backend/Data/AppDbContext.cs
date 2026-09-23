using Microsoft.EntityFrameworkCore;
using MemberEligibility.Api.Models;

namespace MemberEligibility.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Member> Members => Set<Member>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<IdCard> IdCards => Set<IdCard>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimLine> ClaimLines => Set<ClaimLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>().ToTable("members");
        modelBuilder.Entity<Plan>().ToTable("plans");
        modelBuilder.Entity<Enrollment>().ToTable("enrollments");
        modelBuilder.Entity<IdCard>().ToTable("id_cards");
        modelBuilder.Entity<Claim>().ToTable("claims");
        modelBuilder.Entity<ClaimLine>().ToTable("claim_lines");

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Member)
            .WithMany(m => m.Enrollments)
            .HasForeignKey(e => e.MemberId);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Plan)
            .WithMany()
            .HasForeignKey(e => e.PlanId);

        modelBuilder.Entity<Claim>()
            .HasMany(c => c.Lines)
            .WithOne()
            .HasForeignKey(l => l.ClaimId);
    }
}
