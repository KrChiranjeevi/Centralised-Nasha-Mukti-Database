using System.ComponentModel.DataAnnotations;

namespace NashaMuktiDB.DTOs;

public class CreateCentreDto
{
    [Required(ErrorMessage = "Centre name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Centre name must be between 3 and 200 characters")]
    public string CentreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "State ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid State ID is required")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    public string Email { get; set; } = string.Empty;
}

public class UpdateCentreDto
{
    [Required(ErrorMessage = "Centre name is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Centre name must be between 3 and 200 characters")]
    public string CentreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "State ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "A valid State ID is required")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District cannot exceed 100 characters")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class CentreResponseDto
{
    public int CentreId { get; set; }
    public string CentreName { get; set; } = string.Empty;
    public int StateId { get; set; }
    public string StateName { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BeneficiariesCount { get; set; }
}
