namespace Application.Auths.DTOs;

public class UserWithRolesDTO
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public List<string> Roles { get; set; } = new();
}
