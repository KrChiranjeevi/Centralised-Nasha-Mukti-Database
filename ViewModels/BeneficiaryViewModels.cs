using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NashaMuktiDB.ViewModels;

public class BeneficiaryListViewModel
{
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string? Gender { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}

public class BeneficiaryIndexViewModel
{
    public PaginatedList<BeneficiaryListViewModel> Beneficiaries { get; set; } = default!;
    public string? Search { get; set; }
    public int? CentreId { get; set; }
    public string? Status { get; set; } = "all";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<SelectListItem> Centres { get; set; } = new();
    public bool IsAdmin { get; set; }
    public int? UserCentreId { get; set; }
    public string? UserCentreName { get; set; }
    public int TotalBeneficiariesCount { get; set; }
    public int ActiveBeneficiariesCount { get; set; }
}

public class CreateBeneficiaryViewModel
{
    [Required(ErrorMessage = "Beneficiary full name is required")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assigned centre is required")]
    [Display(Name = "Assigned Centre")]
    public int CentreId { get; set; }

    [Required(ErrorMessage = "Registration date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Registration Date")]
    public DateTime RegistrationDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth (Optional)")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
    [Display(Name = "Gender (Optional)")]
    public string? Gender { get; set; }

    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Contact Number (Optional)")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Initial Status")]
    public string Status { get; set; } = "Active";

    public List<SelectListItem> CentresList { get; set; } = new();
    public bool IsAdmin { get; set; }
    public string? AssignedCentreName { get; set; }
}

public class EditBeneficiaryViewModel
{
    [Required]
    public int BeneficiaryId { get; set; }

    [Display(Name = "Beneficiary Code")]
    public string BeneficiaryCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Assigned centre is required")]
    [Display(Name = "Assigned Centre")]
    public int CentreId { get; set; }

    [Required(ErrorMessage = "Registration date is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Registration Date")]
    public DateTime RegistrationDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
    [Display(Name = "Gender")]
    public string? Gender { get; set; }

    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Contact Number")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [Display(Name = "Status")]
    public string Status { get; set; } = "Active";

    public List<SelectListItem> CentresList { get; set; } = new();
    public bool IsAdmin { get; set; }
    public string? AssignedCentreName { get; set; }
}

public class BeneficiaryDetailsViewModel
{
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age => DateOfBirth.HasValue ? (int)((DateTime.Today - DateOfBirth.Value).TotalDays / 365.25) : null;
    public string? Gender { get; set; }
    public string? ContactNumber { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool CanEdit { get; set; }

    // Phase 6: Integrated Clinical Dossier
    public List<CounsellingSummaryItem> CounsellingSessions { get; set; } = new();
    public List<InterventionSummaryItem> Interventions { get; set; } = new();
    public List<FollowUpSummaryItem> FollowUps { get; set; } = new();
}
