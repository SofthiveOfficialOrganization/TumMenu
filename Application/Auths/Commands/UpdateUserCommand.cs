using MediatR;

namespace Application.Auths.Commands;

public class UpdateUserCommand : IRequest<bool>
{
    public string Id { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = null!;
}
