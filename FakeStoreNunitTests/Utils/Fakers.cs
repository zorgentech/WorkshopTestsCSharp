using AutoBogus;
using FakeStore.Model.Domain;

namespace FakeStoreNunitTests.Utils;

public class Fakers
{
    public readonly AutoFaker<Attendant> attendant;
    public readonly AutoFaker<Store> store;
    public readonly AutoFaker<Order> order;

    public Fakers()
    {
        attendant = new AutoFaker<Attendant>();
        attendant.RuleFor(a => a.Orders, () => null);
        attendant.RuleFor(a => a.Stores, () => null);

        store = new AutoFaker<Store>();
        store.RuleFor(s => s.Attendants, () => null);
        store.RuleFor(s => s.Orders, () => null);

        order = new AutoFaker<Order>();
        order.RuleFor(o => o.Attendant, () => attendant.Generate());
        order.RuleFor(o => o.Store, () => store.Generate());
    }
}
