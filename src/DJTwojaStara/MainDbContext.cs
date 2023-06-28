using DJTwojaStara.Models;
using Microsoft.EntityFrameworkCore;

namespace DJTwojaStara;

public sealed class MainDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public MainDbContext(DbContextOptions<MainDbContext> options) : base(options)
    {
        Database.OpenConnection();
        Database.EnsureCreated();
        Database.Migrate();
    }
    
    public DbSet<PerformanceSnapshot> PerformanceSnapshots { get; set; } = null!;
}