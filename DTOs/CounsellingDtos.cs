using System.ComponentModel.DataAnnotations;

namespace NashaMuktiDB.DTOs;

public class CreateCounsellingDto
{
    [Required(ErrorMessage = "Beneficiary ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid Beneficiary ID is required")]
    public int BeneficiaryId { get; set; }

    [Required(ErrorMessage = "Session date is required")]
    [DataType(DataType.Date)]
    public DateTime SessionDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Session type is required")]
    [StringLength(100, ErrorMessage = "Session type cannot exceed 100 characters")]
    public string SessionType { get; set; } = "Individual Counselling";

    [StringLength(150, ErrorMessage = "Counsellor name cannot exceed 150 characters")]
    public string? CounsellorName { get; set; }

    [Required(ErrorMessage = "Session status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string SessionStatus { get; set; } = "Completed";

    [StringLength(2000, ErrorMessage = "Clinical notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

public class UpdateCounsellingDto
{
    [Required(ErrorMessage = "Session date is required")]
    [DataType(DataType.Date)]
    public DateTime SessionDate { get; set; }

    [Required(ErrorMessage = "Session type is required")]
    [StringLength(100, ErrorMessage = "Session type cannot exceed 100 characters")]
    public string SessionType { get; set; } = string.Empty;

    [StringLength(150, ErrorMessage = "Counsellor name cannot exceed 150 characters")]
    public string? CounsellorName { get; set; }

    [Required(ErrorMessage = "Session status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string SessionStatus { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Clinical notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}

public class CounsellingListItemDto
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
    // Confidential notes omitted in lists
}

public class CounsellingDetailDto
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
}
