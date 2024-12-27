using FakeStore.Data;
using FakeStoreNunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreNunitTests;

[SetUpFixture]
public class MySetUpFixture
{
    public static CustomWebApplicationFactory<Program> Factory = new();

    public MySetUpFixture()
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.GetService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
}
