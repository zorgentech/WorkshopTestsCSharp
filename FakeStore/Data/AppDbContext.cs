using FakeStore.Model.Domain;
using Microsoft.EntityFrameworkCore;

namespace FakeStore.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Store> Stores { get; set; }
}
