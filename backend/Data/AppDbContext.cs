using ExciteApi.Models;
using Microsoft.EntityFrameworkCore;

namespace ExciteApi.Data;

public class AppDbContext : DbContext
{
    // 1. Konstruktor
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // 2. Táblák (DbSet) definiálása
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    // 3. Adatbázis inicializálás és Seed adatok
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Fix tesztcsapat automatikus feltöltése a feladatleírás alapján
        modelBuilder.Entity<TeamMember>().HasData(
            new TeamMember { Id = 1, Name = "Alice" },
            new TeamMember { Id = 2, Name = "Bob" },
            new TeamMember { Id = 3, Name = "Charlie" },
            new TeamMember { Id = 4, Name = "Diana" }
        );
    }
}