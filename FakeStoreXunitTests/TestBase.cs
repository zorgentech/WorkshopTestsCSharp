using FakeStore.Data;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreXunitTests;

public class TestBase : IAsyncLifetime
{
    private static bool _databaseCreated = false;
    private IDbContextTransaction _transaction;
    public static readonly CustomWebApplicationFactory<Program> Factory = new();
    public IServiceScope Scope;
    public AppDbContext DbContext;
    public HttpClient Client;

    public TestBase()
    {
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.GetService<AppDbContext>();
        Client = Factory.CreateClient();
        if (!_databaseCreated)
        {
            CreateDatabase();
            _databaseCreated = true;
        }
    }

    public async Task InitializeAsync()
    {
        _transaction = await DbContext.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }

    private void CreateDatabase()
    {
        try
        {
            DbContext.Database.EnsureDeleted();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        try
        {
            DbContext.Database.EnsureCreated();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
