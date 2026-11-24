using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Data;

/// <summary>
/// Entity Framework Core database context for the Centralised Nasha Mukti Database.
/// Integrates ASP.NET Core Identity, Core Directory, and Clinical Modules.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<State> States => Set<State>();
    public DbSet<Centre> Centres => Set<Centre>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();
    public DbSet<CounsellingSession> CounsellingSessions => Set<CounsellingSession>();
    public DbSet<Intervention> Interventions => Set<Intervention>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<AccountRequest> AccountRequests => Set<AccountRequest>();
    public DbSet<CentreServiceSummaryView> CentreServiceSummaries => Set<CentreServiceSummaryView>();
    public DbSet<StateBeneficiarySummaryView> StateBeneficiarySummaries => Set<StateBeneficiarySummaryView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── 1. ApplicationUser Configuration ──────────────────────────────
        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(150);
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.MustChangePassword).HasDefaultValue(false);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(u => u.Centre)
                .WithMany(c => c.StaffMembers)
                .HasForeignKey(u => u.CentreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AspNetUsers_Centres");

            entity.HasIndex(u => u.CentreId).HasDatabaseName("IX_AspNetUsers_CentreId");
        });

        // ── 1B. AccountRequest Configuration ──────────────────────────────
        modelBuilder.Entity<AccountRequest>(entity =>
        {
            entity.ToTable("AccountRequests");
            entity.HasKey(r => r.AccountRequestId);
            entity.Property(r => r.FullName).IsRequired().HasMaxLength(150);
            entity.Property(r => r.Email).IsRequired().HasMaxLength(150);
            entity.Property(r => r.ContactNumber).HasMaxLength(20);
            entity.Property(r => r.ReasonForAccess).IsRequired().HasMaxLength(500);
            entity.Property(r => r.Status).IsRequired().HasMaxLength(30).HasDefaultValue("Pending");
            entity.Property(r => r.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(r => r.AdminNotes).HasMaxLength(500);

            entity.HasOne(r => r.RequestedCentre)
                .WithMany()
                .HasForeignKey(r => r.RequestedCentreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AccountRequests_Centres");

            entity.HasOne(r => r.ReviewedByUser)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AccountRequests_ReviewedBy");

            entity.HasOne(r => r.ApprovedUser)
                .WithMany()
                .HasForeignKey(r => r.ApprovedUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_AccountRequests_ApprovedUser");

            entity.HasIndex(r => r.Email).HasDatabaseName("IX_AccountRequests_Email");
            entity.HasIndex(r => r.Status).HasDatabaseName("IX_AccountRequests_Status");
            entity.HasIndex(r => r.RequestedCentreId).HasDatabaseName("IX_AccountRequests_RequestedCentreId");
        });

        // ── 2. State Entity Configuration ─────────────────────────────────
        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("States");
            entity.HasKey(s => s.StateId);
            entity.Property(s => s.StateName).IsRequired().HasMaxLength(100);
            entity.Property(s => s.IsActive).HasDefaultValue(true);
            entity.HasIndex(s => s.StateName).HasDatabaseName("IX_States_StateName");
        });

        // ── 3. Centre Entity Configuration ────────────────────────────────
        modelBuilder.Entity<Centre>(entity =>
        {
            entity.ToTable("Centres");
            entity.HasKey(c => c.CentreId);
            entity.Property(c => c.CentreName).IsRequired().HasMaxLength(200);
            entity.Property(c => c.District).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Address).IsRequired().HasMaxLength(500);
            entity.Property(c => c.ContactNumber).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            entity.Property(c => c.IsActive).HasDefaultValue(true);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(c => c.State)
                .WithMany(s => s.Centres)
                .HasForeignKey(c => c.StateId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Centres_States");

            entity.HasIndex(c => c.StateId).HasDatabaseName("IX_Centres_StateId");
            entity.HasIndex(c => c.District).HasDatabaseName("IX_Centres_District");
            entity.HasIndex(c => c.CentreName).HasDatabaseName("IX_Centres_CentreName");
            entity.HasIndex(c => c.IsActive).HasDatabaseName("IX_Centres_IsActive");
        });

        // ── 4. Beneficiary Entity Configuration ───────────────────────────
        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("Beneficiaries");
            entity.HasKey(b => b.BeneficiaryId);
            entity.Property(b => b.BeneficiaryCode).IsRequired().HasMaxLength(50);
            entity.Property(b => b.FullName).IsRequired().HasMaxLength(150);
            entity.Property(b => b.RegistrationDate).HasColumnType("date").IsRequired();
            entity.Property(b => b.DateOfBirth).HasColumnType("date");
            entity.Property(b => b.Gender).HasMaxLength(20);
            entity.Property(b => b.ContactNumber).HasMaxLength(20);
            entity.Property(b => b.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Active");
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(b => b.BeneficiaryCode).IsUnique().HasDatabaseName("UQ_Beneficiaries_BeneficiaryCode");

            entity.HasOne(b => b.Centre)
                .WithMany(c => c.Beneficiaries)
                .HasForeignKey(b => b.CentreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Beneficiaries_Centres");

            entity.HasIndex(b => b.CentreId).HasDatabaseName("IX_Beneficiaries_CentreId");
            entity.HasIndex(b => b.FullName).HasDatabaseName("IX_Beneficiaries_FullName");
            entity.HasIndex(b => b.RegistrationDate).HasDatabaseName("IX_Beneficiaries_RegistrationDate");
            entity.HasIndex(b => b.Status).HasDatabaseName("IX_Beneficiaries_Status");
        });

        // ── 5. CounsellingSession Entity Configuration ────────────────────
        modelBuilder.Entity<CounsellingSession>(entity =>
        {
            entity.ToTable("CounsellingSessions");
            entity.HasKey(s => s.SessionId);
            entity.Property(s => s.SessionDate).HasColumnType("date").IsRequired();
            entity.Property(s => s.SessionType).IsRequired().HasMaxLength(100);
            entity.Property(s => s.CounsellorName).HasMaxLength(150);
            entity.Property(s => s.SessionStatus).IsRequired().HasMaxLength(50).HasDefaultValue("Completed");
            entity.Property(s => s.Notes).HasMaxLength(2000);
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationship: Beneficiary 1 -> * CounsellingSessions
            entity.HasOne(s => s.Beneficiary)
                .WithMany(b => b.CounsellingSessions)
                .HasForeignKey(s => s.BeneficiaryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_CounsellingSessions_Beneficiaries");

            entity.HasIndex(s => s.BeneficiaryId).HasDatabaseName("IX_CounsellingSessions_BeneficiaryId");
            entity.HasIndex(s => s.SessionDate).HasDatabaseName("IX_CounsellingSessions_SessionDate");
            entity.HasIndex(s => s.SessionStatus).HasDatabaseName("IX_CounsellingSessions_SessionStatus");
            entity.HasIndex(s => s.SessionType).HasDatabaseName("IX_CounsellingSessions_SessionType");
        });

        // ── 6. Intervention Entity Configuration ──────────────────────────
        modelBuilder.Entity<Intervention>(entity =>
        {
            entity.ToTable("Interventions");
            entity.HasKey(i => i.InterventionId);
            entity.Property(i => i.InterventionType).IsRequired().HasMaxLength(100);
            entity.Property(i => i.ServiceDate).HasColumnType("date").IsRequired();
            entity.Property(i => i.Status).IsRequired().HasMaxLength(50).HasDefaultValue("In Progress");
            entity.Property(i => i.ResponsibleStaffName).HasMaxLength(150);
            entity.Property(i => i.Notes).HasMaxLength(2000);
            entity.Property(i => i.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationship: Beneficiary 1 -> * Interventions
            entity.HasOne(i => i.Beneficiary)
                .WithMany(b => b.Interventions)
                .HasForeignKey(i => i.BeneficiaryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Interventions_Beneficiaries");

            entity.HasIndex(i => i.BeneficiaryId).HasDatabaseName("IX_Interventions_BeneficiaryId");
            entity.HasIndex(i => i.ServiceDate).HasDatabaseName("IX_Interventions_ServiceDate");
            entity.HasIndex(i => i.Status).HasDatabaseName("IX_Interventions_Status");
            entity.HasIndex(i => i.InterventionType).HasDatabaseName("IX_Interventions_InterventionType");
        });

        // ── 7. FollowUp Entity Configuration ──────────────────────────────
        modelBuilder.Entity<FollowUp>(entity =>
        {
            entity.ToTable("FollowUps");
            entity.HasKey(f => f.FollowUpId);
            entity.Property(f => f.ScheduledDate).HasColumnType("date").IsRequired();
            entity.Property(f => f.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(f => f.CompletedDate).HasColumnType("date");
            entity.Property(f => f.Notes).HasMaxLength(1000);
            entity.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relationship: Beneficiary 1 -> * FollowUps
            entity.HasOne(f => f.Beneficiary)
                .WithMany(b => b.FollowUps)
                .HasForeignKey(f => f.BeneficiaryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_FollowUps_Beneficiaries");

            entity.HasIndex(f => f.BeneficiaryId).HasDatabaseName("IX_FollowUps_BeneficiaryId");
            entity.HasIndex(f => f.ScheduledDate).HasDatabaseName("IX_FollowUps_ScheduledDate");
            entity.HasIndex(f => f.Status).HasDatabaseName("IX_FollowUps_Status");
        });

        // ── 8. SQL Server Views (Keyless Entity Types) ─────────────────────
        modelBuilder.Entity<CentreServiceSummaryView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_CentreServiceSummary");
        });

        modelBuilder.Entity<StateBeneficiarySummaryView>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vw_StateBeneficiarySummary");
        });

        // ── 9. Seed Demonstration Data ────────────────────────────────────
        SeedDemonstrationData(modelBuilder);
    }

    private static void SeedDemonstrationData(ModelBuilder modelBuilder)
    {
        // 1. Synthetic States / UTs
        modelBuilder.Entity<State>().HasData(
            new State { StateId = 1, StateName = "Punjab", IsActive = true },
            new State { StateId = 2, StateName = "Bihar", IsActive = true },
            new State { StateId = 3, StateName = "Maharashtra", IsActive = true },
            new State { StateId = 4, StateName = "Rajasthan", IsActive = true },
            new State { StateId = 5, StateName = "Delhi", IsActive = true },
            new State { StateId = 6, StateName = "Madhya Pradesh", IsActive = true },
            new State { StateId = 7, StateName = "Uttar Pradesh", IsActive = true },
            new State { StateId = 8, StateName = "Haryana", IsActive = true }
        );

        // 2. Synthetic De-addiction Centres
        modelBuilder.Entity<Centre>().HasData(
            new Centre
            {
                CentreId = 1,
                CentreName = "Navchetan De-Addiction & Rehabilitation Centre",
                StateId = 1,
                District = "Amritsar",
                Address = "Civil Lines, Near Red Cross Bhawan, Amritsar, Punjab - 143001",
                ContactNumber = "0183-2550101",
                Email = "navchetan.amritsar@example.gov.in",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc)
            },
            new Centre
            {
                CentreId = 2,
                CentreName = "Asha Kiran Counselling and Recovery Centre",
                StateId = 1,
                District = "Ludhiana",
                Address = "Model Town, Phase II, Ludhiana, Punjab - 141002",
                ContactNumber = "0161-2440202",
                Email = "ashakiran.ludhiana@example.gov.in",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new Centre
            {
                CentreId = 3,
                CentreName = "Naya Savera Integrated Rehabilitation Facility",
                StateId = 2,
                District = "Patna",
                Address = "Bailey Road, Near High Court, Patna, Bihar - 800001",
                ContactNumber = "0612-2230303",
                Email = "nayasavera.patna@example.gov.in",
                IsActive = true,
                CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Centre
            {
                CentreId = 4,
                CentreName = "Prerna De-Addiction and Care Kendra",
                StateId = 3,
                District = "Pune",
                Address = "Shivajinagar, J.M. Road, Pune, Maharashtra - 411005",
                ContactNumber = "020-25510404",
                Email = "prerna.pune@example.gov.in",
                IsActive = true,
                CreatedAt = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new Centre
            {
                CentreId = 5,
                CentreName = "Sanjeevani Wellness and Rehabilitation Kendra",
                StateId = 4,
                District = "Jaipur",
                Address = "Tonk Road, Gandhi Nagar, Jaipur, Rajasthan - 302015",
                ContactNumber = "0141-2700505",
                Email = "sanjeevani.jaipur@example.gov.in",
                IsActive = true,
                CreatedAt = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 3. Synthetic Beneficiaries
        modelBuilder.Entity<Beneficiary>().HasData(
            new Beneficiary
            {
                BeneficiaryId = 1,
                BeneficiaryCode = "BEN-PB-2026-0001",
                CentreId = 1,
                FullName = "Gurpreet Singh (Demo)",
                DateOfBirth = new DateTime(1992, 5, 14),
                RegistrationDate = new DateTime(2026, 2, 1),
                Gender = "Male",
                ContactNumber = "9876543210",
                Status = "Active",
                CreatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Beneficiary
            {
                BeneficiaryId = 2,
                BeneficiaryCode = "BEN-PB-2026-0002",
                CentreId = 1,
                FullName = "Jaswinder Kaur (Demo)",
                DateOfBirth = new DateTime(1988, 11, 20),
                RegistrationDate = new DateTime(2026, 2, 3),
                Gender = "Female",
                ContactNumber = "9876543211",
                Status = "Active",
                CreatedAt = new DateTime(2026, 2, 3, 0, 0, 0, DateTimeKind.Utc)
            },
            new Beneficiary
            {
                BeneficiaryId = 3,
                BeneficiaryCode = "BEN-PB-2026-0003",
                CentreId = 2,
                FullName = "Harmanpreet Sharma (Demo)",
                DateOfBirth = new DateTime(1995, 8, 9),
                RegistrationDate = new DateTime(2026, 2, 10),
                Gender = "Male",
                ContactNumber = "9876543212",
                Status = "Active",
                CreatedAt = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new Beneficiary
            {
                BeneficiaryId = 4,
                BeneficiaryCode = "BEN-BR-2026-0001",
                CentreId = 3,
                FullName = "Ramesh Kumar Verma (Demo)",
                DateOfBirth = new DateTime(1985, 3, 17),
                RegistrationDate = new DateTime(2026, 2, 12),
                Gender = "Male",
                ContactNumber = "9876543213",
                Status = "Active",
                CreatedAt = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc)
            },
            new Beneficiary
            {
                BeneficiaryId = 5,
                BeneficiaryCode = "BEN-MH-2026-0001",
                CentreId = 4,
                FullName = "Sachin Raghunath Patil (Demo)",
                DateOfBirth = new DateTime(1990, 7, 25),
                RegistrationDate = new DateTime(2026, 2, 14),
                Gender = "Male",
                ContactNumber = "9876543214",
                Status = "Active",
                CreatedAt = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc)
            },
            new Beneficiary
            {
                BeneficiaryId = 6,
                BeneficiaryCode = "BEN-RJ-2026-0001",
                CentreId = 5,
                FullName = "Vikram Singh Rathore (Demo)",
                DateOfBirth = new DateTime(1993, 12, 4),
                RegistrationDate = new DateTime(2026, 2, 18),
                Gender = "Male",
                ContactNumber = "9876543215",
                Status = "Active",
                CreatedAt = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 4. Synthetic Counselling Sessions (Demonstration records)
        modelBuilder.Entity<CounsellingSession>().HasData(
            new CounsellingSession
            {
                SessionId = 1,
                BeneficiaryId = 1,
                SessionDate = new DateTime(2026, 2, 5),
                SessionType = "Individual",
                CounsellorName = "Dr. Harvinder Singh (Psychologist)",
                SessionStatus = "Completed",
                Notes = "Initial motivational interviewing completed. Beneficiary expressed readiness for rehabilitation.",
                CreatedAt = new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc)
            },
            new CounsellingSession
            {
                SessionId = 2,
                BeneficiaryId = 1,
                SessionDate = new DateTime(2026, 2, 12),
                SessionType = "Family",
                CounsellorName = "Dr. Harvinder Singh (Psychologist)",
                SessionStatus = "Completed",
                Notes = "Family session conducted with spouse. Established positive household recovery support framework.",
                CreatedAt = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc)
            },
            new CounsellingSession
            {
                SessionId = 3,
                BeneficiaryId = 2,
                SessionDate = new DateTime(2026, 2, 8),
                SessionType = "Individual",
                CounsellorName = "Dr. Manpreet Kaur (Counsellor)",
                SessionStatus = "Completed",
                Notes = "Cognitive behavioral therapy intro. Identified primary emotional triggers and coping strategies.",
                CreatedAt = new DateTime(2026, 2, 8, 0, 0, 0, DateTimeKind.Utc)
            },
            new CounsellingSession
            {
                SessionId = 4,
                BeneficiaryId = 4,
                SessionDate = new DateTime(2026, 2, 15),
                SessionType = "Group",
                CounsellorName = "Sanjay Jha (Social Worker)",
                SessionStatus = "Completed",
                Notes = "Participated in peer support group. Shared challenges and received positive group feedback.",
                CreatedAt = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 5. Synthetic Interventions
        modelBuilder.Entity<Intervention>().HasData(
            new Intervention
            {
                InterventionId = 1,
                BeneficiaryId = 1,
                InterventionType = "Medical Detoxification",
                ServiceDate = new DateTime(2026, 2, 2),
                Status = "Completed",
                ResponsibleStaffName = "Dr. Rajesh Sharma (Medical Officer)",
                Notes = "5-day inpatient clinical detox successfully administered without acute complications.",
                CreatedAt = new DateTime(2026, 2, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new Intervention
            {
                InterventionId = 2,
                BeneficiaryId = 1,
                InterventionType = "Psychosocial Rehabilitation",
                ServiceDate = new DateTime(2026, 2, 10),
                Status = "In Progress",
                ResponsibleStaffName = "Navchetan Rehabilitation Team",
                Notes = "Enrolled in 60-day residential psychosocial recovery module.",
                CreatedAt = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new Intervention
            {
                InterventionId = 3,
                BeneficiaryId = 4,
                InterventionType = "Medical Detoxification",
                ServiceDate = new DateTime(2026, 2, 13),
                Status = "Completed",
                ResponsibleStaffName = "Dr. P.K. Sinha (Physician)",
                Notes = "Supervised pharmacotherapy detox provided at Patna facility.",
                CreatedAt = new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 6. Synthetic Follow-Ups
        modelBuilder.Entity<FollowUp>().HasData(
            new FollowUp
            {
                FollowUpId = 1,
                BeneficiaryId = 1,
                ScheduledDate = new DateTime(2026, 3, 1),
                Status = "Pending",
                CompletedDate = null,
                Notes = "1-month post-intake recovery evaluation scheduled.",
                CreatedAt = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc)
            },
            new FollowUp
            {
                FollowUpId = 2,
                BeneficiaryId = 2,
                ScheduledDate = new DateTime(2026, 2, 20),
                Status = "Completed",
                CompletedDate = new DateTime(2026, 2, 20),
                Notes = "Routine bi-weekly check-in completed. Beneficiary reports good adherence to program.",
                CreatedAt = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
            },
            new FollowUp
            {
                FollowUpId = 3,
                BeneficiaryId = 4,
                ScheduledDate = new DateTime(2026, 2, 15),
                Status = "Missed",
                CompletedDate = null,
                Notes = "Beneficiary was unable to attend due to travel. Re-contact initiated by field team.",
                CreatedAt = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
