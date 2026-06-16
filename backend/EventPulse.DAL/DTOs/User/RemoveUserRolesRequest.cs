using System.ComponentModel.DataAnnotations;

namespace EventPulse.BLL.DTOs.User;

public class RemoveUserRolesRequest
{
    [Required, MinLength(1)]
    public List<int> RoleIds { get; set; } = new();
}
