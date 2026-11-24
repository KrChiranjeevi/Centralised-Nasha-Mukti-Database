using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Status enumeration for registration access requests.
/// </summary>
public enum AccountRequestStatus
{
    Pending,
    Approved,
    Rejected
}

/// <summary>
/// Represents an official request for portal access submitted by prospective centre staff
/// or administrators, subject to review and approval by a National Administrator.
/// </summary>
public class AccountRequest
{
    [Key]
    public int AccountRequestId { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Official email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    [Display(Name = "Official Email Address")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Please select your requested centre")]
    [Display(Name = "Requested Centre")]
    public int RequestedCentreId { get; set; }

    [ForeignKey(nameof(RequestedCentreId))]
    public virtual Centre? RequestedCentre { get; set; }

    [Required(ErrorMessage = "Reason for access is required")]
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    [Display(Name = "Reason for Access")]
    public string ReasonForAccess { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    [Display(Name = "Status")]
    public string Status { get; set; } = nameof(AccountRequestStatus.Pending);

    [Display(Name = "Requested On")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Reviewed On")]
    public DateTime? ReviewedAt { get; set; }

    [StringLength(450)]
    [Display(Name = "Reviewed By")]
    public string? ReviewedByUserId { get; set; }

    [ForeignKey(nameof(ReviewedByUserId))]
    public virtual ApplicationUser? ReviewedByUser { get; set; }

    [StringLength(450)]
    [Display(Name = "Created User Account")]
    public string? ApprovedUserId { get; set; }

    [ForeignKey(nameof(ApprovedUserId))]
    public virtual ApplicationUser? ApprovedUser { get; set; }

    [StringLength(500)]
    [Display(Name = "Administrator Remarks")]
    public string? AdminNotes { get; set; }
}
