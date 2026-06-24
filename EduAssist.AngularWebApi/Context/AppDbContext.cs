using EduAssist.AngularWebApi.Models;
using EduAssist.AngularWebApi.Seed;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserRequest> UserRequests { get; set; }
    public DbSet<AIResponse> AIResponses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.SeedCategories();
    }
}
