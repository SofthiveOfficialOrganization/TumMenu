using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class CustomerOrderRequestItem : BaseEntity
{
    public Guid OrderRequestId { get; set; }
    public CustomerOrderRequest OrderRequest { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductPriceId { get; set; }
    public ProductPrice? ProductPrice { get; set; }

    [MaxLength(200)]
    public string ProductTitleSnapshot { get; set; } = null!;

    [MaxLength(100)]
    public string? ProductPriceSizeSnapshot { get; set; }

    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}
