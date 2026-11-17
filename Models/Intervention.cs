using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Represents a medical, psychological, or rehabilitative intervention delivered to a beneficiary.
/// Categories include Detoxification, Pharmacotherapy, Psychosocial Support, Referral, etc.
/// </summary>
public class Intervention
{
    [Key]
    public int InterventionId { get; set; }

    [Required(ErrorMessage = "Beneficiary is required")]
    [Display(Name = "Beneficiary")]
    public int BeneficiaryId { get; set; }

    [Required(ErrorMessage = "Intervention category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    [Display(Name = "Intervention Category")]
    public string InterventionType { get; set; } = "Medical Detoxification";

    [Required(ErrorMessage = "Service date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Service Date")]
    public DateTime ServiceDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Intervention Status")]
    public string Status { get; set; } = "In Progress"; // In Progress, Completed, Discontinued, Referred

    [StringLength(150, ErrorMessage = "Responsible professional cannot exceed 150 characters")]
    [Display(Name = "Responsible Professional / Staff")]
    public string? ResponsibleStaffName { get; set; }

    [StringLength(2000, ErrorMessage = "Service notes cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Intervention & Treatment Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Recorded At")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Updated")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation property: Belongs to one Beneficiary
    [ForeignKey(nameof(BeneficiaryId))]
    public virtual Beneficiary? Beneficiary { get; set; }
}

