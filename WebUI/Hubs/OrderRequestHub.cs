using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace WebUI.Hubs;

[Authorize(Policy = "OwnerOrAdmin")]
public sealed class OrderRequestHub(ApplicationDbContext db) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var companyId = await db.Companies
                .Where(x => x.Owner != null && x.Owner.ApplicationUserId == userId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(Context.ConnectionAborted);

            if (companyId.HasValue)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    GroupName(companyId.Value),
                    Context.ConnectionAborted);
            }
        }

        await base.OnConnectedAsync();
    }

    public static string GroupName(Guid companyId) => $"company:{companyId:N}";
}
