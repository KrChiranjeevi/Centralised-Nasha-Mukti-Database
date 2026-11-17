using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Represents a Nasha Mukti Kendra / de-addiction facility supported under MoSJE scheme.
/// </summary>
public class Centre
{
    [Key]
    public int CentreId { get; set; }

    [Required(ErrorMessage = "Centre name is required")]
    [StringLength(200, ErrorMessage = "Centre name cannot exceed 200 characters")]
    [Display(Name = "Centre Name")]
    public string CentreName { get; set; } = string.Empty;

    [Required(ErrorMessage = "State is required")]
    [Display(Name = "State")]
    public int StateId { get; set; }

    [Required(ErrorMessage = "District is required")]
    [StringLength(100, ErrorMessage = "District name cannot exceed 100 characters")]
    [Display(Name = "District")]
    public string District { get; set; } = string.Empty;

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    [Display(Name = "Complete Address")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact number is required")]
    [StringLength(20, ErrorMessage = "Contact number cannot exceed 20 characters")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Contact Number")]
    public string ContactNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required")]
    [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Status")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Registered On")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: Belongs to one State
    [ForeignKey(nameof(StateId))]
    public virtual State? State { get; set; }

    // Navigation property: Centre has many Beneficiaries
    public virtual ICollection<Beneficiary> Beneficiaries { get; set; } = new List<Beneficiary>();

    // Navigation property: Centre has assigned Staff members
    public virtual ICollection<ApplicationUser> StaffMembers { get; set; } = new List<ApplicationUser>();
}
