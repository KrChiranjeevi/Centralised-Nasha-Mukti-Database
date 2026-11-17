using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NashaMuktiDB.Models;

/// <summary>
/// Represents an Indian state or union territory under which Nasha Mukti Kendras operate.
/// </summary>
public class State
{
    [Key]
    public int StateId { get; set; }

    [Required(ErrorMessage = "State name is required")]
    [StringLength(100, ErrorMessage = "State name cannot exceed 100 characters")]
    [Display(Name = "State / UT Name")]
    public string StateName { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    // Navigation property: One State has many Centres
    public virtual ICollection<Centre> Centres { get; set; } = new List<Centre>();
}
