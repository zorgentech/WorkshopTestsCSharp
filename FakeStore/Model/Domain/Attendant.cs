namespace FakeStore.Model.Domain;

public class Attendant
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    // navigation attributes
    public ICollection<Order>? Orders { get; set; }
    public ICollection<Store>? Stores { get; set; }
}
