using System.ComponentModel.DataAnnotations;

namespace NashaMuktiDB.ViewModels;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address (Username)")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "Temporary Password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirm password is required")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Password and confirmation do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    [Display(Name = "User Role")]
    public string Role { get; set; } = "CentreStaff";

    [Display(Name = "Assigned Centre")]
    public int? CentreId { get; set; }
}

