namespace EventPulse.BLL.DTOs.User;

public class UserListResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<int> RoleIds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
