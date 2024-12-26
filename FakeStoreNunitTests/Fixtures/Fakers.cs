using AutoBogus;
using FakeStore.Model.Domain;

namespace FakeStoreNunitTests.Fixtures;

public class Fakers
{
    public AutoFaker<Attendant> attendantFaker;
    public AutoFaker<Store> storeFaker;
    public AutoFaker<Order> orderFaker;

    public Fakers()
    {
        attendantFaker = new AutoFaker<Attendant>();
        attendantFaker.RuleFor(a => a.Orders, () => []);
        attendantFaker.RuleFor(a => a.Stores, () => []);

        storeFaker = new AutoFaker<Store>();
        storeFaker.RuleFor(s => s.Attendants, () => []);
        storeFaker.RuleFor(s => s.Orders, () => []);

        orderFaker = new AutoFaker<Order>();
        orderFaker.RuleFor(
            o => o.Attendant,
            () =>
            {
                return attendantFaker.Generate();
            }
        );
        orderFaker.RuleFor(
            o => o.Store,
            () =>
            {
                return storeFaker.Generate();
            }
        );
    }
}
