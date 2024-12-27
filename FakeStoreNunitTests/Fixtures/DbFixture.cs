using FakeStore.Data;
using FakeStoreNunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreNunitTests;

[SetUpFixture]
public class DbFixture
{
    public static CustomWebApplicationFactory<Program> Factory = new();

    public DbFixture()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.GetService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}
