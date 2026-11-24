using System.ComponentModel.DataAnnotations;

namespace NashaMuktiDB.ViewModels;

/// <summary>
/// ViewModel for submitting an official request for portal access.
/// Submitted by prospective staff members or centre administrators.
/// </summary>
public class RequestAccountViewModel
{
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
    [Phone(ErrorMessage = "Please enter a valid contact number")]
    [Display(Name = "Contact Number (Optional)")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Please select the de-addiction centre you are affiliated with")]
    [Display(Name = "Affiliated De-Addiction Centre")]
    public int? RequestedCentreId { get; set; }

    [Required(ErrorMessage = "Reason for access request is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Reason must be between 10 and 500 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Reason for System Access")]
    public string ReasonForAccess { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for displaying registration requests in the Admin User Management module.
/// </summary>
public class AccountRequestItemViewModel
{
    public int AccountRequestId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public int RequestedCentreId { get; set; }
    public string RequestedCentreName { get; set; } = string.Empty;
    public string ReasonForAccess { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByName { get; set; }
    public string? AdminNotes { get; set; }
    public string? ApprovedUserId { get; set; }
}

/// <summary>
/// ViewModel for Administrator review and approval of an access request.
/// </summary>
public class ApproveAccountRequestViewModel
{
    public int AccountRequestId { get; set; }

    // Read-only applicant details for context
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public int RequestedCentreId { get; set; }
    public string RequestedCentreName { get; set; } = string.Empty;
    public string ReasonForAccess { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Admin assignment decisions
    [Required(ErrorMessage = "Assigned role is required")]
    [Display(Name = "System Role")]
    public string AssignedRole { get; set; } = "CentreStaff";

    [Display(Name = "Authorized Centre")]
    public int? AssignedCentreId { get; set; }

    [Required(ErrorMessage = "Initial temporary password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "Initial Temporary Password")]
    public string TemporaryPassword { get; set; } = "Temp@NMD2026!";

    [Required(ErrorMessage = "Please confirm the initial password")]
    [DataType(DataType.Password)]
    [Compare(nameof(TemporaryPassword), ErrorMessage = "Password and confirmation do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = "Temp@NMD2026!";

    [Display(Name = "Require Password Change On First Login")]
    public bool RequirePasswordChange { get; set; } = true;

    [StringLength(500, ErrorMessage = "Admin notes cannot exceed 500 characters")]
    [Display(Name = "Administrator Remarks / Verification Notes")]
    public string? AdminNotes { get; set; }
}

/// <summary>
/// ViewModel for Administrator rejection of an access request.
/// </summary>
public class RejectAccountRequestViewModel
{
    public int AccountRequestId { get; set; }

    [Required(ErrorMessage = "Please provide a reason for declining this request")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Rejection note must be between 5 and 500 characters")]
    [Display(Name = "Reason for Decline")]
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for requiring an approved user to establish a permanent password.
/// </summary>
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current temporary password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Current Temporary Password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "New Permanent Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your new password")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "New password and confirmation do not match")]
    [Display(Name = "Confirm New Password")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
