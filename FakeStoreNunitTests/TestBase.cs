using FakeStore.Data;
using FakeStoreNunitTests.Extensions;
using FakeStoreNunitTests.Utils;
using FakeStoreXunitTests.Factories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreNunitTests;

public class TestBase
{
    private IDbContextTransaction _transaction;
    public CustomWebApplicationFactory<Program> Factory;
    public IServiceScope Scope;
    public AppDbContext DbContext;
    public HttpClient Client;
    public Fakers Fakers = new();

    [SetUp]
    public async Task InitializeAsync()
    {
        Factory = MySetUpFixture.Factory;
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.GetService<AppDbContext>();
        Client = Factory.CreateClient();
        _transaction = await DbContext.Database.BeginTransactionAsync();
    }

    [TearDown]
    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        await DbContext.DisposeAsync();
        Client.Dispose();
        Scope.Dispose();
    }
}
