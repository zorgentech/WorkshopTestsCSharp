using AutoBogus;
using FakeStore.Model.Domain;

namespace FakeStoreXunitTests.Fixtures;

public class Fakers
{
    public AutoFaker<Attendant> attendant;
    public AutoFaker<Store> store;
    public AutoFaker<Order> order;

    public Fakers()
    {
        attendant = new AutoFaker<Attendant>();
        attendant.RuleFor(a => a.Orders, () => []);
        attendant.RuleFor(a => a.Stores, () => []);

        store = new AutoFaker<Store>();
        store.RuleFor(s => s.Attendants, () => []);
        store.RuleFor(s => s.Orders, () => []);

        order = new AutoFaker<Order>();
        order.RuleFor(o => o.Attendant, () => attendant.Generate());
        order.RuleFor(o => o.Store, () => store.Generate());
    }
}
