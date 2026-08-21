using Domain.Entities;

namespace Application.CustomerOrderRequests.DTOs;

public sealed class QrOrderSessionDTO
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid QRCodeId { get; set; }
    public string Token { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class QrOrderSessionStatusDTO
{
    public bool IsValid { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

public sealed class CreateCustomerOrderRequestItemDTO
{
    public Guid ProductId { get; set; }
    public Guid? ProductPriceId { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}

public class CustomerOrderRequestSummaryDTO
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public string TableNumber { get; set; } = null!;
    public CustomerOrderRequestStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public int ItemCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class CustomerOrderRequestDetailDTO : CustomerOrderRequestSummaryDTO
{
    public string? Note { get; set; }
    public List<CustomerOrderRequestItemDTO> Items { get; set; } = [];
}

public sealed class CustomerOrderRequestItemDTO
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductPriceId { get; set; }
    public string ProductTitle { get; set; } = null!;
    public string? ProductPriceSize { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Note { get; set; }
}
