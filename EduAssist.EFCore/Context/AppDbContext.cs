using EduAssist.EFCore.Models;
using EduAssist.EFCore.Seed;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.EFCore.Context;

/// <summary>
/// Core Database Context - Contains business tables:
/// UserRequests, AIResponses, Categories, Bookmarks.
/// Separate from the Authentication database.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserRequest> UserRequests { get; set; }
    public DbSet<AIResponse> AIResponses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from the Configurations folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Seed default categories (academic subjects)
        modelBuilder.SeedCategories();
    }
}
