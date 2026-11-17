using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace NashaMuktiDB.Models;

/// <summary>
/// Custom application user extending ASP.NET Core IdentityUser.
/// Supports role-based access for National Administrators and Centre Staff.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(150, ErrorMessage = "Full name cannot exceed 150 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to Centre. Null for global Administrators;
    /// Required for CentreStaff to enforce centre-level data segregation.
    /// </summary>
    [Display(Name = "Assigned Centre")]
    public int? CentreId { get; set; }

    [Display(Name = "Account Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Account Created")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property: The centre this staff member belongs to
    [ForeignKey(nameof(CentreId))]
    public virtual Centre? Centre { get; set; }
}

