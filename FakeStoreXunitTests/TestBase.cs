using FakeStore.Data;
using FakeStoreXunitTests.Extensions;
using FakeStoreXunitTests.Factories;
using FakeStoreXunitTests.Utils;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreXunitTests;

/// <summary>
/// Responsável por inicializar a factory e criar o banco de dados
/// </summary>
public class TestBaseSetUp
{
    public CustomApplicationFactory<Program> Factory = new();

    public TestBaseSetUp()
    {
        using var scope = Factory.Services.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        // se estivermos usando migrations, chamar dbContext.Database.Migrate pois o coverage pega as migrations
        dbContext.Database.EnsureCreated();
    }
}

/// <summary>
/// Serve como storage para que as classes de setup iniciem somente uma vez
/// </summary>
public class SharedSetUps
{
    // TODO: explicar porque não usar com IClassFixture na classe base de teste
    public static readonly TestBaseSetUp TestBaseSetUp = new();
    public static readonly Fakers Fakers = new();
}

/// <summary>
/// Contém tudo o que é necessário para os testes de integração
/// também é responsável pela limpeza do banco depois de cada teste
/// </summary>
public abstract class TestBase : IAsyncLifetime
{
    private IDbContextTransaction _transaction;
    public CustomApplicationFactory<Program> Factory = SharedSetUps.TestBaseSetUp.Factory;
    public Fakers Fakers = SharedSetUps.Fakers;
    public IServiceScope Scope;
    public AppDbContext DbContext;
    public HttpClient Client;

    public TestBase()
    {
        Scope = Factory.Services.CreateAsyncScope();
        Client = Factory.CreateClient();
        DbContext = Scope.GetService<AppDbContext>();
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
