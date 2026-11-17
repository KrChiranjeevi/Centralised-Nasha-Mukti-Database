using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NashaMuktiDB.ViewModels;

// ═══════════════════════════════════════════════════════════════════════════
// COUNSELLING SESSION VIEW MODELS
// ═══════════════════════════════════════════════════════════════════════════

public class CounsellingSessionListItemViewModel
{
    public int SessionId { get; set; }
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public string? CounsellorName { get; set; }
    public string SessionStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CounsellingIndexViewModel
{
    public PaginatedList<CounsellingSessionListItemViewModel> Sessions { get; set; } = null!;
    public string? SearchQuery { get; set; }
    public int? BeneficiaryId { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? SessionType { get; set; }
    public string? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TotalSessionsCount { get; set; }
    public int CompletedCount { get; set; }
    public int ScheduledCount { get; set; }
    public List<SelectListItem> TypesList { get; set; } = new();
    public List<SelectListItem> StatusesList { get; set; } = new();
}

public class CreateCounsellingViewModel
{
    [Required(ErrorMessage = "Please select a beneficiary")]
    [Display(Name = "Beneficiary")]
    public int BeneficiaryId { get; set; }

    public string? BeneficiaryCode { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? CentreName { get; set; }

    [Required(ErrorMessage = "Session date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Session Date")]
    public DateTime SessionDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Session type is required")]
    [StringLength(100, ErrorMessage = "Session type cannot exceed 100 characters")]
    [Display(Name = "Session Type")]
    public string SessionType { get; set; } = "Individual Counselling";

    [StringLength(150, ErrorMessage = "Counsellor name cannot exceed 150 characters")]
    [Display(Name = "Counsellor / Therapist")]
    public string? CounsellorName { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Session Status")]
    public string SessionStatus { get; set; } = "Completed";

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Clinical Notes (Confidential)")]
    public string? Notes { get; set; }

    public List<SelectListItem> BeneficiariesList { get; set; } = new();
    public List<SelectListItem> TypesList { get; set; } = new();
    public List<SelectListItem> StatusesList { get; set; } = new();
}

public class EditCounsellingViewModel
{
    [Required]
    public int SessionId { get; set; }

    [Required]
    public int BeneficiaryId { get; set; }

    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Session date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Session Date")]
    public DateTime SessionDate { get; set; }

    [Required(ErrorMessage = "Session type is required")]
    [StringLength(100, ErrorMessage = "Session type cannot exceed 100 characters")]
    [Display(Name = "Session Type")]
    public string SessionType { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Counsellor name cannot exceed 150 characters")]
    [Display(Name = "Counsellor / Therapist")]
    public string? CounsellorName { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Session Status")]
    public string SessionStatus { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Clinical Notes (Confidential)")]
    public string? Notes { get; set; }

    public List<SelectListItem> TypesList { get; set; } = new();
    public List<SelectListItem> StatusesList { get; set; } = new();
}

public class CounsellingDetailsViewModel
{
    public int SessionId { get; set; }
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public DateTime SessionDate { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public string? CounsellorName { get; set; }
    public string SessionStatus { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// INTERVENTION VIEW MODELS
// ═══════════════════════════════════════════════════════════════════════════

public class InterventionListItemViewModel
{
    public int InterventionId { get; set; }
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public string InterventionType { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResponsibleStaffName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class InterventionIndexViewModel
{
    public PaginatedList<InterventionListItemViewModel> Interventions { get; set; } = null!;
    public string? SearchQuery { get; set; }
    public int? BeneficiaryId { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? InterventionType { get; set; }
    public string? Status { get; set; }
    public int TotalInterventionsCount { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public List<SelectListItem> TypesList { get; set; } = new();
    public List<SelectListItem> StatusesList { get; set; } = new();
}

public class CreateInterventionViewModel
{
    [Required(ErrorMessage = "Please select a beneficiary")]
    [Display(Name = "Beneficiary")]
    public int BeneficiaryId { get; set; }

    public string? BeneficiaryCode { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? CentreName { get; set; }

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
    [Display(Name = "Status")]
    public string Status { get; set; } = "In Progress";

    [StringLength(150, ErrorMessage = "Responsible professional cannot exceed 150 characters")]
    [Display(Name = "Responsible Professional / Specialist")]
    public string? ResponsibleStaffName { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Clinical & Rehabilitation Notes")]
    public string? Notes { get; set; }

    public List<SelectListItem> BeneficiariesList { get; set; } = new();
    public List<SelectListItem> TypesList { get; set; } = new();
    public List<SelectListItem> StatusesList { get; set; } = new();
}

public class EditInterventionViewModel
{
    [Required]
    public int InterventionId { get; set; }

    [Required]
    public int BeneficiaryId { get; set; }

    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Intervention category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    [Display(Name = "Intervention Category")]
    public string InterventionType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Service date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Service Date")]
    public DateTime ServiceDate { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Responsible professional cannot exceed 150 characters")]
    [Display(Name = "Responsible Professional / Specialist")]
    public string? ResponsibleStaffName { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Clinical & Rehabilitation Notes")]
    public string? Notes { get; set; }

    public List<SelectListItem> TypesList { get; set; } = new();
    public List<SelectListItem> StatusesList { get; set; } = new();
}

public class InterventionDetailsViewModel
{
    public int InterventionId { get; set; }
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string InterventionType { get; set; } = string.Empty;
    public DateTime ServiceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResponsibleStaffName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// FOLLOW-UP VIEW MODELS
// ═══════════════════════════════════════════════════════════════════════════

public class FollowUpListItemViewModel
{
    public int FollowUpId { get; set; }
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public bool IsOverdue => Status == "Pending" && ScheduledDate.Date < DateTime.Today;
    public DateTime CreatedAt { get; set; }
}

public class FollowUpIndexViewModel
{
    public PaginatedList<FollowUpListItemViewModel> FollowUps { get; set; } = null!;
    public string? SearchQuery { get; set; }
    public string FilterTab { get; set; } = "all"; // all, pending, overdue, completed
    public string? Status { get; set; }
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int OverdueCount { get; set; }
    public int CompletedCount { get; set; }
    public List<SelectListItem> StatusesList { get; set; } = new();
}

public class CreateFollowUpViewModel
{
    [Required(ErrorMessage = "Please select a beneficiary")]
    [Display(Name = "Beneficiary")]
    public int BeneficiaryId { get; set; }

    public string? BeneficiaryCode { get; set; }
    public string? BeneficiaryName { get; set; }
    public string? CentreName { get; set; }

    [Required(ErrorMessage = "Scheduled date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Scheduled Follow-up Date")]
    public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(14);

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Pending";

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Follow-up Instructions / Notes")]
    public string? Notes { get; set; }

    public List<SelectListItem> BeneficiariesList { get; set; } = new();
}

public class CompleteFollowUpViewModel
{
    [Required]
    public int FollowUpId { get; set; }

    [Required]
    public int BeneficiaryId { get; set; }

    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }

    [Required(ErrorMessage = "Completion date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Date Conducted / Completed")]
    public DateTime CompletedDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Outcome status is required")]
    [Display(Name = "Follow-up Outcome Status")]
    public string Status { get; set; } = "Completed"; // Completed, Missed, Cancelled

    [StringLength(1000, ErrorMessage = "Outcome notes cannot exceed 1000 characters")]
    [DataType(DataType.MultilineText)]
    [Display(Name = "Outcome & Recovery Notes (Confidential)")]
    public string? Notes { get; set; }
}

public class FollowUpDetailsViewModel
{
    public int FollowUpId { get; set; }
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public string CentreName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsOverdue => Status == "Pending" && ScheduledDate.Date < DateTime.Today;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool CanEdit { get; set; }
}

// ═══════════════════════════════════════════════════════════════════════════
// CLINICAL SUMMARY ITEMS FOR BENEFICIARY DOSSIER
// ═══════════════════════════════════════════════════════════════════════════

public class CounsellingSummaryItem
{
    public int SessionId { get; set; }
    public DateTime SessionDate { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public string? CounsellorName { get; set; }
    public string SessionStatus { get; set; } = string.Empty;
}

public class InterventionSummaryItem
{
    public int InterventionId { get; set; }
    public DateTime ServiceDate { get; set; }
    public string InterventionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ResponsibleStaffName { get; set; }
}

public class FollowUpSummaryItem
{
    public int FollowUpId { get; set; }
    public DateTime ScheduledDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }
    public bool IsOverdue => Status == "Pending" && ScheduledDate.Date < DateTime.Today;
}

