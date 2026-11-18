using System.ComponentModel.DataAnnotations;

namespace NashaMuktiDB.DTOs;

public class CreateFollowUpDto
{
    [Required(ErrorMessage = "Beneficiary ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid Beneficiary ID is required")]
    public int BeneficiaryId { get; set; }

    [Required(ErrorMessage = "Scheduled date is required")]
    [DataType(DataType.Date)]
    public DateTime ScheduledDate { get; set; } = DateTime.Today.AddDays(14);

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = "Pending";

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

public class UpdateFollowUpDto
{
    [Required(ErrorMessage = "Scheduled date is required")]
    [DataType(DataType.Date)]
    public DateTime ScheduledDate { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = "Completed";

    [DataType(DataType.Date)]
    public DateTime? CompletedDate { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}

public class FollowUpResponseDto
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
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

