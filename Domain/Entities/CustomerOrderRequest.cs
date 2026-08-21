using Domain.Base;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public enum CustomerOrderRequestStatus
{
    New = 1,
    Seen = 2,
    Completed = 3
}

public class CustomerOrderRequest : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public Guid StoreId { get; set; }
    public Store Store { get; set; } = null!;

    public Guid QrOrderSessionId { get; set; }
    public QrOrderSession QrOrderSession { get; set; } = null!;

    [MaxLength(150)]
    public string CustomerName { get; set; } = null!;

    [MaxLength(40)]
    public string TableNumber { get; set; } = null!;

    [MaxLength(1000)]
    public string? Note { get; set; }

    public CustomerOrderRequestStatus Status { get; set; } = CustomerOrderRequestStatus.New;
    public decimal Subtotal { get; set; }
    public int ItemCount { get; set; }

    public ICollection<CustomerOrderRequestItem> Items { get; set; } = [];
}
