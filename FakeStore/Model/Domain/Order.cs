using FakeStore.Model.Enums;

namespace FakeStore.Model.Domain;

public class Order
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public Guid StoreId { get; set; }
    public Store Store { get; set; }
}
