using FakeStore.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FakeStoreXunitTests.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a new DbContext using a teste database
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite("Data Source=test_database.db");
            });
        });
    }
}
