using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Represents a structured counselling session conducted with a beneficiary.
/// Confidential clinical notes are access-restricted.
/// </summary>
public class CounsellingSession
{
    [Key]
    public int SessionId { get; set; }

    [Required(ErrorMessage = "Beneficiary is required")]
    [Display(Name = "Beneficiary")]
    public int BeneficiaryId { get; set; }

    [Required(ErrorMessage = "Session date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Session Date")]
    public DateTime SessionDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Session type is required")]
    [StringLength(100, ErrorMessage = "Session type cannot exceed 100 characters")]
    [Display(Name = "Session Type")]
    public string SessionType { get; set; } = "Individual"; // Individual, Group, Family, Relapse Prevention, Crisis Intervention

    [StringLength(150, ErrorMessage = "Counsellor name cannot exceed 150 characters")]
    [Display(Name = "Counsellor / Therapist")]
    public string? CounsellorName { get; set; }

    [Required(ErrorMessage = "Session status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Session Status")]
    public string SessionStatus { get; set; } = "Completed"; // Completed, Scheduled, Cancelled, No-Show

    [StringLength(2000, ErrorMessage = "Confidential notes cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Clinical Notes (Confidential)")]
    public string? Notes { get; set; }

    [Display(Name = "Recorded At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property: Belongs to one Beneficiary
    [ForeignKey(nameof(BeneficiaryId))]
    public virtual Beneficiary? Beneficiary { get; set; }
}

