using EduAssist.AngularWebApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduAssist.AngularWebApi.Context;

public class AuthDbContext : IdentityDbContext<ApplicationUser>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(u => u.Institution).HasMaxLength(300);
            entity.Property(u => u.Grade).HasMaxLength(50);
            entity.Property(u => u.Bio).HasMaxLength(500);
            entity.Property(u => u.PreferredLanguage).HasMaxLength(10).HasDefaultValue("en");
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.Property(u => u.TotalQueriesAsked).HasDefaultValue(0);
            entity.Property(u => u.JoinedOn).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(u => u.LastActiveOn).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
