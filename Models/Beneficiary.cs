using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Represents a registered beneficiary receiving counselling or de-addiction services.
/// Uses a synthetic, non-sensitive unique BeneficiaryCode (e.g. BEN-PB-2026-0001).
/// </summary>
public class Beneficiary
{
    [Key]
    public int BeneficiaryId { get; set; }

    [Required(ErrorMessage = "Beneficiary code is required")]
    [StringLength(50, ErrorMessage = "Beneficiary code cannot exceed 50 characters")]
    [Display(Name = "Beneficiary ID")]
    public string BeneficiaryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assigned centre is required")]
    [Display(Name = "Assigned Centre")]
    public int CentreId { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Registration date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Registration Date")]
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow.Date;

    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Active";

    [Display(Name = "Created At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property: Belongs to one Centre
    [ForeignKey(nameof(CentreId))]
    public virtual Centre? Centre { get; set; }

    // Navigation properties for clinical modules
    public virtual ICollection<CounsellingSession> CounsellingSessions { get; set; } = new List<CounsellingSession>();
    public virtual ICollection<Intervention> Interventions { get; set; } = new List<Intervention>();
    public virtual ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
}
