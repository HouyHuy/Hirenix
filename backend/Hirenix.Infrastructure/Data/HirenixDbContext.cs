using Hirenix.Domain.Entities;
using Hirenix.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Hirenix.Infrastructure.Data;

public class HirenixDbContext : DbContext
{
    public HirenixDbContext(DbContextOptions<HirenixDbContext> options) : base(options) { }

    // ─── Auth ─────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ─── Taxonomy ─────────────────────────────────────────────────────
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<Location> Locations => Set<Location>();

    // ─── Company ──────────────────────────────────────────────────────
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<EmployerProfile> EmployerProfiles => Set<EmployerProfile>();

    // ─── Candidate ────────────────────────────────────────────────────
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<CandidateEducation> CandidateEducations => Set<CandidateEducation>();
    public DbSet<CandidateExperience> CandidateExperiences => Set<CandidateExperience>();

    // ─── Job ──────────────────────────────────────────────────────────
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobSkill> JobSkills => Set<JobSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ════════════════════════════════════════════════════════════════
        //  USERS
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Email).HasColumnName("email").HasMaxLength(255);
            e.Property(u => u.Phone).HasColumnName("phone").HasMaxLength(20);
            e.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            e.Property(u => u.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.AuthProvider).HasColumnName("auth_provider").HasConversion<string>().HasMaxLength(20);
            e.Property(u => u.AuthProviderId).HasColumnName("auth_provider_id").HasMaxLength(255);
            e.Property(u => u.OtpCode).HasColumnName("otp_code").HasMaxLength(10);
            e.Property(u => u.OtpExpiresAt).HasColumnName("otp_expires_at");
            e.Property(u => u.IsActive).HasColumnName("is_active");
            e.Property(u => u.IsVerified).HasColumnName("is_verified");
            e.Property(u => u.FailedLoginAttempts).HasColumnName("failed_login_attempts").HasDefaultValue(0);
            e.Property(u => u.LockoutEnd).HasColumnName("lockout_end");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
            e.Property(u => u.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Phone).IsUnique();
            e.HasIndex(u => new { u.AuthProvider, u.AuthProviderId });
        });

        // ════════════════════════════════════════════════════════════════
        //  REFRESH TOKENS
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("id");
            e.Property(r => r.UserId).HasColumnName("user_id");
            e.Property(r => r.Token).HasColumnName("token").HasMaxLength(512);
            e.Property(r => r.ExpiresAt).HasColumnName("expires_at");
            e.Property(r => r.CreatedAt).HasColumnName("created_at");
            e.Property(r => r.IsRevoked).HasColumnName("is_revoked");
        });

        // ════════════════════════════════════════════════════════════════
        //  TAXONOMY
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Skill>(e =>
        {
            e.ToTable("skills");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(s => s.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            e.Property(s => s.Category).HasColumnName("category").HasMaxLength(100);
            e.HasIndex(s => s.Name).IsUnique();
            e.HasIndex(s => s.Slug).IsUnique();
        });

        modelBuilder.Entity<Industry>(e =>
        {
            e.ToTable("industries");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("id");
            e.Property(i => i.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(i => i.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            e.HasIndex(i => i.Name).IsUnique();
            e.HasIndex(i => i.Slug).IsUnique();
        });

        modelBuilder.Entity<Location>(e =>
        {
            e.ToTable("locations");
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasColumnName("id");
            e.Property(l => l.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            e.Property(l => l.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
            e.Property(l => l.CountryCode).HasColumnName("country_code").HasMaxLength(2).HasDefaultValue("VN");
            e.HasIndex(l => l.Slug).IsUnique();
        });

        // ════════════════════════════════════════════════════════════════
        //  COMPANIES
        // ════════════════════════════════════════════════════════════════
        var sizeToString = new Dictionary<CompanySize, string>
        {
            { CompanySize.Size_1_10, "1-10" },
            { CompanySize.Size_11_50, "11-50" },
            { CompanySize.Size_51_200, "51-200" },
            { CompanySize.Size_201_500, "201-500" },
            { CompanySize.Size_500Plus, "500+" }
        };
        var stringToSize = sizeToString.ToDictionary(x => x.Value, x => x.Key);

        var companySizeConverter = new ValueConverter<CompanySize?, string?>(
            v => v.HasValue && sizeToString.ContainsKey(v.Value) ? sizeToString[v.Value] : null,
            v => v != null && stringToSize.ContainsKey(v) ? stringToSize[v] : null
        );

        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("companies");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
            e.Property(c => c.LogoUrl).HasColumnName("logo_url").HasColumnType("text");
            e.Property(c => c.Description).HasColumnName("description").HasColumnType("text");
            e.Property(c => c.Website).HasColumnName("website").HasMaxLength(255);
            e.Property(c => c.Size).HasColumnName("size").HasConversion(companySizeConverter).HasMaxLength(20);
            e.Property(c => c.IndustryId).HasColumnName("industry_id");
            e.Property(c => c.CityId).HasColumnName("city_id");
            e.Property(c => c.Address).HasColumnName("address").HasColumnType("text");
            e.Property(c => c.IsVerified).HasColumnName("is_verified");
            e.Property(c => c.IsActive).HasColumnName("is_active");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
            e.Property(c => c.UpdatedAt).HasColumnName("updated_at");

            e.HasOne(c => c.Industry).WithMany().HasForeignKey(c => c.IndustryId);
            e.HasOne(c => c.City).WithMany().HasForeignKey(c => c.CityId);
        });

        modelBuilder.Entity<EmployerProfile>(e =>
        {
            e.ToTable("employer_profiles");
            e.HasKey(ep => ep.Id);
            e.Property(ep => ep.Id).HasColumnName("id");
            e.Property(ep => ep.UserId).HasColumnName("user_id");
            e.Property(ep => ep.CompanyId).HasColumnName("company_id");
            e.Property(ep => ep.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
            e.Property(ep => ep.Title).HasColumnName("title").HasMaxLength(100);
            e.Property(ep => ep.IsAdmin).HasColumnName("is_admin");
            e.Property(ep => ep.IsActive).HasColumnName("is_active");
            e.Property(ep => ep.CreatedAt).HasColumnName("created_at");

            e.HasIndex(ep => ep.UserId).IsUnique();
            e.HasOne(ep => ep.User).WithMany().HasForeignKey(ep => ep.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ep => ep.Company).WithMany(c => c.EmployerProfiles).HasForeignKey(ep => ep.CompanyId).OnDelete(DeleteBehavior.Cascade);
        });

        // ════════════════════════════════════════════════════════════════
        //  CANDIDATE PROFILES
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<CandidateProfile>(e =>
        {
            e.ToTable("candidate_profiles");
            e.HasKey(cp => cp.Id);
            e.Property(cp => cp.Id).HasColumnName("id");
            e.Property(cp => cp.UserId).HasColumnName("user_id");
            e.Property(cp => cp.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
            e.Property(cp => cp.AvatarUrl).HasColumnName("avatar_url").HasColumnType("text");
            e.Property(cp => cp.Gender).HasColumnName("gender").HasConversion<string>().HasMaxLength(10);
            e.Property(cp => cp.DateOfBirth).HasColumnName("date_of_birth");
            e.Property(cp => cp.Address).HasColumnName("address").HasColumnType("text");
            e.Property(cp => cp.CityId).HasColumnName("city_id");
            e.Property(cp => cp.ExpectedSalaryMin).HasColumnName("expected_salary_min");
            e.Property(cp => cp.ExpectedSalaryMax).HasColumnName("expected_salary_max");
            e.Property(cp => cp.DesiredPosition).HasColumnName("desired_position").HasMaxLength(255);
            e.Property(cp => cp.WorkType).HasColumnName("work_type").HasConversion<string>().HasMaxLength(20);
            e.Property(cp => cp.Level).HasColumnName("level").HasConversion<string>().HasMaxLength(20);
            e.Property(cp => cp.IndustryId).HasColumnName("industry_id");
            e.Property(cp => cp.IsOpenToWork).HasColumnName("is_open_to_work");
            e.Property(cp => cp.IsProfileHidden).HasColumnName("is_profile_hidden");
            e.Property(cp => cp.Bio).HasColumnName("bio").HasColumnType("text");
            e.Property(cp => cp.CreatedAt).HasColumnName("created_at");
            e.Property(cp => cp.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(cp => cp.UserId).IsUnique();
            e.HasOne(cp => cp.User).WithMany().HasForeignKey(cp => cp.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cp => cp.City).WithMany().HasForeignKey(cp => cp.CityId);
            e.HasOne(cp => cp.Industry).WithMany().HasForeignKey(cp => cp.IndustryId);
        });

        modelBuilder.Entity<CandidateSkill>(e =>
        {
            e.ToTable("candidate_skills");
            e.HasKey(cs => cs.Id);
            e.Property(cs => cs.Id).HasColumnName("id");
            e.Property(cs => cs.CandidateId).HasColumnName("candidate_id");
            e.Property(cs => cs.SkillId).HasColumnName("skill_id");
            e.Property(cs => cs.Level).HasColumnName("level").HasConversion<string>().HasMaxLength(20);

            e.HasIndex(cs => new { cs.CandidateId, cs.SkillId }).IsUnique();
            e.HasOne(cs => cs.Candidate).WithMany(c => c.Skills).HasForeignKey(cs => cs.CandidateId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(cs => cs.Skill).WithMany().HasForeignKey(cs => cs.SkillId);
        });

        modelBuilder.Entity<CandidateEducation>(e =>
        {
            e.ToTable("candidate_educations");
            e.HasKey(ce => ce.Id);
            e.Property(ce => ce.Id).HasColumnName("id");
            e.Property(ce => ce.CandidateId).HasColumnName("candidate_id");
            e.Property(ce => ce.SchoolName).HasColumnName("school_name").HasMaxLength(255).IsRequired();
            e.Property(ce => ce.Degree).HasColumnName("degree").HasMaxLength(100);
            e.Property(ce => ce.Major).HasColumnName("major").HasMaxLength(255);
            e.Property(ce => ce.StartYear).HasColumnName("start_year");
            e.Property(ce => ce.EndYear).HasColumnName("end_year");
            e.Property(ce => ce.Description).HasColumnName("description").HasColumnType("text");

            e.HasOne(ce => ce.Candidate).WithMany(c => c.Educations).HasForeignKey(ce => ce.CandidateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CandidateExperience>(e =>
        {
            e.ToTable("candidate_experiences");
            e.HasKey(cx => cx.Id);
            e.Property(cx => cx.Id).HasColumnName("id");
            e.Property(cx => cx.CandidateId).HasColumnName("candidate_id");
            e.Property(cx => cx.CompanyName).HasColumnName("company_name").HasMaxLength(255).IsRequired();
            e.Property(cx => cx.Position).HasColumnName("position").HasMaxLength(255).IsRequired();
            e.Property(cx => cx.StartDate).HasColumnName("start_date");
            e.Property(cx => cx.EndDate).HasColumnName("end_date");
            e.Property(cx => cx.IsCurrent).HasColumnName("is_current");
            e.Property(cx => cx.Description).HasColumnName("description").HasColumnType("text");

            e.HasOne(cx => cx.Candidate).WithMany(c => c.Experiences).HasForeignKey(cx => cx.CandidateId).OnDelete(DeleteBehavior.Cascade);
        });

        // ════════════════════════════════════════════════════════════════
        //  JOBS
        // ════════════════════════════════════════════════════════════════
        modelBuilder.Entity<Job>(e =>
        {
            e.ToTable("jobs");
            e.HasKey(j => j.Id);
            e.Property(j => j.Id).HasColumnName("id");
            e.Property(j => j.CompanyId).HasColumnName("company_id");
            e.Property(j => j.CreatedBy).HasColumnName("created_by");
            e.Property(j => j.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            e.Property(j => j.Description).HasColumnName("description").HasColumnType("text").IsRequired();
            e.Property(j => j.Requirements).HasColumnName("requirements").HasColumnType("text");
            e.Property(j => j.Benefits).HasColumnName("benefits").HasColumnType("text");
            e.Property(j => j.WorkType).HasColumnName("work_type").HasConversion<string>().HasMaxLength(20);
            e.Property(j => j.Level).HasColumnName("level").HasConversion<string>().HasMaxLength(20);
            e.Property(j => j.SalaryMin).HasColumnName("salary_min");
            e.Property(j => j.SalaryMax).HasColumnName("salary_max");
            e.Property(j => j.IsSalaryVisible).HasColumnName("is_salary_visible");
            e.Property(j => j.CityId).HasColumnName("city_id");
            e.Property(j => j.IndustryId).HasColumnName("industry_id");
            e.Property(j => j.Deadline).HasColumnName("deadline");
            e.Property(j => j.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            e.Property(j => j.IsFeatured).HasColumnName("is_featured");
            e.Property(j => j.ViewsCount).HasColumnName("views_count");
            e.Property(j => j.ApplicationsCount).HasColumnName("applications_count");
            e.Property(j => j.ParentJobId).HasColumnName("parent_job_id");
            e.Property(j => j.CreatedAt).HasColumnName("created_at");
            e.Property(j => j.UpdatedAt).HasColumnName("updated_at");

            e.HasIndex(j => j.Status);
            e.HasIndex(j => j.CompanyId);
            e.HasIndex(j => j.Deadline);
            e.HasOne(j => j.Company).WithMany(c => c.Jobs).HasForeignKey(j => j.CompanyId);
            e.HasOne(j => j.Creator).WithMany().HasForeignKey(j => j.CreatedBy);
            e.HasOne(j => j.City).WithMany().HasForeignKey(j => j.CityId);
            e.HasOne(j => j.Industry).WithMany().HasForeignKey(j => j.IndustryId);
            e.HasOne(j => j.ParentJob).WithMany().HasForeignKey(j => j.ParentJobId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<JobSkill>(e =>
        {
            e.ToTable("job_skills");
            e.HasKey(js => js.Id);
            e.Property(js => js.Id).HasColumnName("id");
            e.Property(js => js.JobId).HasColumnName("job_id");
            e.Property(js => js.SkillId).HasColumnName("skill_id");
            e.Property(js => js.IsRequired).HasColumnName("is_required");

            e.HasIndex(js => new { js.JobId, js.SkillId }).IsUnique();
            e.HasOne(js => js.Job).WithMany(j => j.Skills).HasForeignKey(js => js.JobId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(js => js.Skill).WithMany().HasForeignKey(js => js.SkillId);
        });
    }
}
