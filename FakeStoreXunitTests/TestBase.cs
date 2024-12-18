using FakeStore.Data;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using FakeStoreXunitTests.Fixtures;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreXunitTests;

public class TestBase : IClassFixture<DbFixture>, IAsyncLifetime
{
    private IDbContextTransaction _transaction;
    public IServiceScope Scope;
    public CustomWebApplicationFactory<Program> Factory;
    public AppDbContext DbContext;
    public HttpClient Client;

    public TestBase(DbFixture dbFixture)
    {
        Factory = dbFixture.Factory;
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.GetService<AppDbContext>();
        Client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _transaction = await DbContext.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.RollbackAsync();
        await _transaction.DisposeAsync();
        Client.Dispose();
        DbContext.Dispose();
        Scope.Dispose();
    }
}
