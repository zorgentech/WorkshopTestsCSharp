using FakeStore.Data;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using FakeStoreXunitTests.Utils;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreXunitTests;

/// <summary>
/// Contém tudo o que é necessário para os testes de integração
/// também é responsável pela limpeza do banco depois de cada teste
/// </summary>
public abstract class TestBase : IAsyncLifetime
{
    private IDbContextTransaction _transaction;
    public static readonly CustomApplicationFactory Factory = new();
    public static readonly Fakers Fakers = new();
    public readonly IServiceScope Scope;
    public readonly AppDbContext DbContext;
    public readonly HttpClient Client;

    static TestBase()
    {
        CreateDatabase();
    }

    public TestBase()
    {
        Scope = Factory.Services.CreateAsyncScope();
        Client = Factory.CreateClient();
        DbContext = Scope.GetService<AppDbContext>();
    }

    public static void CreateDatabase()
    {
        using var scope = Factory.Services.CreateAsyncScope();
        using var dbContext = scope.GetService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        // se estivermos usando migrations, chamar dbContext.Database.Migrate pois o coverage pega as migrations
        dbContext.Database.EnsureCreated();
    }

    public async Task InitializeAsync()
    {
        _transaction = await DbContext.Database.BeginTransactionAsync();
    }

    public async Task DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}
