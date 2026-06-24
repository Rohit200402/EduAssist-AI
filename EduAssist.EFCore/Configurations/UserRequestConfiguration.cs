using EduAssist.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.EFCore.Configurations;

/// <summary>
/// Fluent API configuration for the UserRequest entity.
/// Defines relationships, constraints, and indexes.
/// </summary>
public class UserRequestConfiguration : IEntityTypeConfiguration<UserRequest>
{
    public void Configure(EntityTypeBuilder<UserRequest> builder)
    {
        // Table name
        builder.ToTable("UserRequests");

        // Primary Key
        builder.HasKey(ur => ur.UserRequestId);

        // Properties
        builder.Property(ur => ur.UserRequestId)
            .ValueGeneratedOnAdd();

        builder.Property(ur => ur.Query)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ur => ur.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(ur => ur.RequestedOn)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships

        // Many UserRequests belong to one Category
        builder.HasOne(ur => ur.Category)
            .WithMany(c => c.UserRequests)
            .HasForeignKey(ur => ur.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Many UserRequests belong to one User (cross-database reference by UserId string)
        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRequests)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One UserRequest has many AIResponses (regeneration supported)
        builder.HasMany(ur => ur.AIResponses)
            .WithOne(ar => ar.UserRequest)
            .HasForeignKey(ar => ar.UserRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ur => ur.UserId)
            .HasDatabaseName("IX_UserRequests_UserId");

        builder.HasIndex(ur => ur.CategoryId)
            .HasDatabaseName("IX_UserRequests_CategoryId");

        builder.HasIndex(ur => ur.RequestedOn)
            .HasDatabaseName("IX_UserRequests_RequestedOn");
    }
}
