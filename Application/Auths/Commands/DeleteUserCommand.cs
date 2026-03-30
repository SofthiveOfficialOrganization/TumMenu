using MediatR;

namespace Application.Auths.Commands;

public class DeleteUserCommand : IRequest<bool>
{
    public string Id { get; set; } = null!;
}
