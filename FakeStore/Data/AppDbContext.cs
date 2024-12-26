using FakeStore.Model.Domain;
using Microsoft.EntityFrameworkCore;

namespace FakeStore.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Store> Stores { get; set; }
    public DbSet<Attendant> Attendants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // seed data, create a Store
        modelBuilder
            .Entity<Store>()
            .HasData(
                new Store
                {
                    Id = Guid.NewGuid(),
                    Name = "My Store",
                    Address = "123 Main St",
                    OrderCancelationLimitInMinutes = 60,
                }
            );
    }
}
