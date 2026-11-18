using System.ComponentModel.DataAnnotations;

namespace NashaMuktiDB.DTOs;

public class CreateInterventionDto
{
    [Required(ErrorMessage = "Beneficiary ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid Beneficiary ID is required")]
    public int BeneficiaryId { get; set; }

    [Required(ErrorMessage = "Intervention category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string InterventionType { get; set; } = "Medical Detoxification";

    [Required(ErrorMessage = "Service date is required")]
    [DataType(DataType.Date)]
    public DateTime ServiceDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = "In Progress";

    [StringLength(150, ErrorMessage = "Responsible professional cannot exceed 150 characters")]
    public string? ResponsibleStaffName { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

public class UpdateInterventionDto
{
    [Required(ErrorMessage = "Intervention category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string InterventionType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Service date is required")]
    [DataType(DataType.Date)]
    public DateTime ServiceDate { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Responsible professional cannot exceed 150 characters")]
    public string? ResponsibleStaffName { get; set; }

    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

public class InterventionResponseDto
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
}

