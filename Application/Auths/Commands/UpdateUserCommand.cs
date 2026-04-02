using Application.Abstractions;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Auths.Commands;

public class UpdateUserCommand : IRequest<bool>, IAuditableCommand
{
    public string Id { get; set; } = null!;
    public string ActionName => "Kullanıcı güncellendi";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = null!;
}

public class UpdateUserCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<UpdateUserCommand, bool>
{
    public async Task<bool> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id);
        if (user == null) return false;

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}
