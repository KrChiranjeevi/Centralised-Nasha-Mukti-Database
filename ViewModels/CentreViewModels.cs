using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NashaMuktiDB.ViewModels;

public class CentreListViewModel
{
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int BeneficiaryCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CentreIndexViewModel
{
    public PaginatedList<CentreListViewModel> Centres { get; set; } = default!;
    public string? Search { get; set; }
    public int? StateId { get; set; }
    public string? Status { get; set; } = "all";
    public List<SelectListItem> States { get; set; } = new();
    public bool IsAdmin { get; set; }
    public int TotalCentresCount { get; set; }
    public int ActiveCentresCount { get; set; }
}

public class CreateCentreViewModel
{
    [Required(ErrorMessage = "Centre name is required")]
    [StringLength(200, ErrorMessage = "Centre name cannot exceed 200 characters")]
    [Display(Name = "Centre Name")]
    public string CentreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [Display(Name = "State / Union Territory")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District name cannot exceed 100 characters")]
    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Complete address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Display(Name = "Complete Physical Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Official Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Official email address is required")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Official Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; } = true;

    public List<SelectListItem> StatesList { get; set; } = new();
}

public class EditCentreViewModel
{
    [Required]
    public int CentreId { get; set; }

    [Required(ErrorMessage = "Centre name is required")]
    [StringLength(200, ErrorMessage = "Centre name cannot exceed 200 characters")]
    [Display(Name = "Centre Name")]
    public string CentreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [Display(Name = "State / Union Territory")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District name cannot exceed 100 characters")]
    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Complete address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Display(Name = "Complete Physical Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [Display(Name = "Official Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Official email address is required")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Official Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; }

    public List<SelectListItem> StatesList { get; set; } = new();
}

public class CentreDetailsViewModel
{
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public string StateName { get; set; } = string.Empty;
    public int StateId { get; set; }
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalBeneficiaries { get; set; }
    public int ActiveBeneficiaries { get; set; }
    public int TotalStaffMembers { get; set; }
    public List<CentreBeneficiarySummary> RecentBeneficiaries { get; set; } = new();
    public List<CentreStaffSummary> StaffMembers { get; set; } = new();
}

public class CentreBeneficiarySummary
{
    public int BeneficiaryId { get; set; }
    public string BeneficiaryCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime RegistrationDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CentreStaffSummary
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

