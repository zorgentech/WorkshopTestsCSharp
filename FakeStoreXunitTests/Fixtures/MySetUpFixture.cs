using FakeStore.Data;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreXunitTests.Fixtures;

public class MySetUpFixture
{
    public readonly CustomWebApplicationFactory<Program> Factory;

    public MySetUpFixture()
    {
        Factory = new CustomWebApplicationFactory<Program>();
        CreateAndSeedDatabase();
    }

    private void CreateAndSeedDatabase()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.GetService<AppDbContext>();
        try
        {
            db.Database.EnsureDeleted();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        try
        {
            db.Database.EnsureCreated();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
