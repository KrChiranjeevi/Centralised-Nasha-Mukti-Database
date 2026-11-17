namespace NashaMuktiDB.ViewModels;

public class UserListViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? CentreId { get; set; }
    public string? CentreName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

