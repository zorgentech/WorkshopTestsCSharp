using FakeStore.Data;
using FakeStoreNunitTests.Extensions;
using FakeStoreNunitTests.Utils;
using FakeStoreXunitTests.Factories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreNunitTests;

public class TestBase
{
    private static bool _databaseCreated = false;
    private IDbContextTransaction _transaction;
    public static readonly CustomWebApplicationFactory<Program> Factory = new();
    public IServiceScope Scope;
    public AppDbContext DbContext;
    public HttpClient Client;
    public Fakers Fakers = new();

    [SetUp]
    public void SetUp()
    {
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.GetService<AppDbContext>();
        Client = Factory.CreateClient();
        if (!_databaseCreated)
        {
            CreateDatabase();
            _databaseCreated = true;
        }
        _transaction = DbContext.Database.BeginTransaction();
    }

    [TearDown]
    public void TearDown()
    {
        _transaction.Dispose();
        Client.Dispose();
        DbContext.Dispose();
        Scope.Dispose();
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
