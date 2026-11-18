using System.ComponentModel.DataAnnotations;

namespace NashaMuktiDB.DTOs;

public class RegisterBeneficiaryDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Centre ID. Required for Administrator callers. For CentreStaff, automatically set to caller's assigned facility.
    /// </summary>
    public int? CentreId { get; set; }

    [DataType(DataType.Date)]
    public DateTime? RegistrationDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
    public string? Gender { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    public string? ContactNumber { get; set; }
}

public class UpdateBeneficiaryDto
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 150 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Centre ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid Centre ID is required")]
    public int CentreId { get; set; }

    [Required(ErrorMessage = "Registration date is required")]
    [DataType(DataType.Date)]
    public DateTime RegistrationDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters")]
    public string? Gender { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    public string? ContactNumber { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = "Active";
}

public class BeneficiaryResponseDto
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
    public string? Gender { get; set; }
    public string? ContactNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

