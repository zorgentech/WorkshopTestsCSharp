using FakeStore.Data;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreXunitTests.Fixtures;

public class DbFixture
{
    public readonly CustomWebApplicationFactory<Program> Factory;
    private readonly IServiceScope Scope;
    private readonly AppDbContext DbContext;

    public DbFixture()
    {
        Factory = new CustomWebApplicationFactory<Program>();
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.GetService<AppDbContext>();
        CreateAndSeedDatabase();
    }

    private void DeleteDatabase()
    {
        DbContext.Database.EnsureDeleted();
    }

    private void CreateAndSeedDatabase()
    {
        try
        {
            DbContext.Database.EnsureDeleted();
        }
        catch { }
        DbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        DeleteDatabase();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
