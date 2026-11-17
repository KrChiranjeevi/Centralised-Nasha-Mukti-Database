using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Represents a post-treatment or ongoing recovery follow-up appointment.
/// Tracks Pending, Completed, Missed, and Cancelled status.
/// </summary>
public class FollowUp
{
    [Key]
    public int FollowUpId { get; set; }

    [Required(ErrorMessage = "Beneficiary is required")]
    [Display(Name = "Beneficiary")]
    public int BeneficiaryId { get; set; }

    [Required(ErrorMessage = "Scheduled date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Scheduled Follow-up Date")]
    public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(14);

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Follow-up Status")]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Missed, Cancelled

    [DataType(DataType.Date)]
    [Display(Name = "Completion Date")]
    public DateTime? CompletedDate { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Outcome & Recovery Notes (Confidential)")]
    public string? Notes { get; set; }

    [Display(Name = "Scheduled At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property: Belongs to one Beneficiary
    [ForeignKey(nameof(BeneficiaryId))]
    public virtual Beneficiary? Beneficiary { get; set; }
}

