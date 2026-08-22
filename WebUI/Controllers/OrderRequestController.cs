using Application.CustomerOrderRequests;
using Application.CustomerOrderRequests.Commands;
using Application.CustomerOrderRequests.DTOs;
using Application.CustomerOrderRequests.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebUI.Hubs;

namespace WebUI.Controllers;

[AllowAnonymous]
public sealed class OrderRequestController(
    ISender sender,
    IHubContext<OrderRequestHub> hubContext) : Controller
{
    [HttpGet("/siparis/oturum")]
    public async Task<IActionResult> Session(Guid? storeId, CancellationToken ct)
    {
        var token = Request.Cookies[OrderSessionToken.CookieName];
        var result = await sender.Send(new ValidateQrOrderSessionQuery(token, storeId), ct);
        return Json(new
        {
            isValid = result.IsValid,
            storeId = result.StoreId,
            companyId = result.CompanyId,
            expiresAt = result.ExpiresAt
        });
    }

    [HttpPost("/siparis/olustur")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Create([FromBody] CreateCustomerOrderRequestCommand command, CancellationToken ct)
    {
        command.SessionToken = Request.Cookies[OrderSessionToken.CookieName];
        var order = await sender.Send(command, ct);

        var payload = new
        {
            id = order.Id,
            storeId = order.StoreId,
            storeName = order.StoreName,
            customerName = order.CustomerName,
            tableNumber = order.TableNumber,
            subtotal = order.Subtotal,
            itemCount = order.ItemCount,
            createdAt = order.CreatedAt,
            status = order.Status.ToString(),
            items = order.Items.Select(item => new
            {
                id = item.Id,
                productId = item.ProductId,
                productPriceId = item.ProductPriceId,
                productTitle = item.ProductTitle,
                productPriceSize = item.ProductPriceSize,
                quantity = item.Quantity,
                unitPrice = item.UnitPrice,
                lineTotal = item.LineTotal,
                note = item.Note
            })
        };

        await hubContext.Clients
            .Group(OrderRequestHub.GroupName(order.CompanyId))
            .SendAsync("orderRequestCreated", payload, ct);

        return Json(new
        {
            success = true,
            message = "Sipariş talebiniz işletmeye iletildi.",
            orderId = order.Id
        });
    }
}
