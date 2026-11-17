using Microsoft.EntityFrameworkCore;
using NashaMuktiDB.Models;

namespace NashaMuktiDB.Data;

/// <summary>
/// Entity Framework Core database context for the Centralised Nasha Mukti Database.
/// Implements Code-First configuration, relationships, constraints, and indexes.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<State> States => Set<State>();
    public DbSet<Centre> Centres => Set<Centre>();
    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── State Entity Configuration ───────────────────────────────
        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("States");

            entity.HasKey(s => s.StateId);

            entity.Property(s => s.StateName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(s => s.StateName)
                .HasDatabaseName("IX_States_StateName");
        });

        // ── Centre Entity Configuration ──────────────────────────────
        modelBuilder.Entity<Centre>(entity =>
        {
            entity.ToTable("Centres");

            entity.HasKey(c => c.CentreId);

            entity.Property(c => c.CentreName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(c => c.District)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Address)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(c => c.ContactNumber)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.IsActive)
                .HasDefaultValue(true);

            entity.Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship: State 1 -> * Centre
            // DeleteBehavior.Restrict: Prevents accidental cascade delete of historical facility records
            entity.HasOne(c => c.State)
                .WithMany(s => s.Centres)
                .HasForeignKey(c => c.StateId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Centres_States");

            // Indexes for common queries (filter by state, district, active status)
            entity.HasIndex(c => c.StateId)
                .HasDatabaseName("IX_Centres_StateId");

            entity.HasIndex(c => c.District)
                .HasDatabaseName("IX_Centres_District");

            entity.HasIndex(c => c.CentreName)
                .HasDatabaseName("IX_Centres_CentreName");

            entity.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Centres_IsActive");
        });

        // ── Beneficiary Entity Configuration ─────────────────────────
        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("Beneficiaries");

            entity.HasKey(b => b.BeneficiaryId);

            entity.Property(b => b.BeneficiaryCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(b => b.FullName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(b => b.RegistrationDate)
                .HasColumnType("date")
                .IsRequired();

            entity.Property(b => b.DateOfBirth)
                .HasColumnType("date");

            entity.Property(b => b.Gender)
                .HasMaxLength(20);

            entity.Property(b => b.ContactNumber)
                .HasMaxLength(20);

            entity.Property(b => b.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            entity.Property(b => b.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // CRITICAL: Unique constraint on BeneficiaryCode (non-sensitive unique public ID)
            entity.HasIndex(b => b.BeneficiaryCode)
                .IsUnique()
                .HasDatabaseName("UQ_Beneficiaries_BeneficiaryCode");

            // Relationship: Centre 1 -> * Beneficiary
            // DeleteBehavior.Restrict: Protects historical patient records from accidental centre deletion
            entity.HasOne(b => b.Centre)
                .WithMany(c => c.Beneficiaries)
                .HasForeignKey(b => b.CentreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Beneficiaries_Centres");

            // Search and filtering indexes
            entity.HasIndex(b => b.CentreId)
                .HasDatabaseName("IX_Beneficiaries_CentreId");

            entity.HasIndex(b => b.FullName)
                .HasDatabaseName("IX_Beneficiaries_FullName");

            entity.HasIndex(b => b.RegistrationDate)
                .HasDatabaseName("IX_Beneficiaries_RegistrationDate");

            entity.HasIndex(b => b.Status)
                .HasDatabaseName("IX_Beneficiaries_Status");
        });

        // ── Seed Demonstration Data (Reproducible EF Core HasData) ────
        SeedDemonstrationData(modelBuilder);
    }

    /// <summary>
    /// Seeds safe, synthetic demonstration data for academic and evaluation purposes.
    /// No actual patient or government information is stored.
    /// </summary>
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
                StateId = 1, // Punjab
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
                StateId = 1, // Punjab
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
                StateId = 2, // Bihar
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
                StateId = 3, // Maharashtra
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
                StateId = 4, // Rajasthan
                District = "Jaipur",
                Address = "Tonk Road, Gandhi Nagar, Jaipur, Rajasthan - 302015",
                ContactNumber = "0141-2700505",
                Email = "sanjeevani.jaipur@example.gov.in",
                IsActive = true,
                CreatedAt = new DateTime(2026, 2, 15, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // 3. Synthetic Beneficiaries (Demonstration records with unique identifiers)
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
    }
}
