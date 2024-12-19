namespace FakeStore.Model.Domain;

public class Attendant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public ICollection<Order> Orders { get; set; }
}
