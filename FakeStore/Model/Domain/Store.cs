namespace FakeStore.Model.Domain;

public class Store
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public int OrderCancelationLimitInMinutes { get; set; }

    // Navigation attributes
    public ICollection<Order>? Orders { get; set; }
    public ICollection<Attendant>? Attendants { get; set; }
}
