using EduAssist.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.EFCore.Configurations;

/// <summary>
/// Fluent API configuration for the Bookmark entity.
/// Defines constraints, relationships, and composite unique index.
/// </summary>
public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        // Table name
        builder.ToTable("Bookmarks");

        // Primary Key
        builder.HasKey(b => b.BookmarkId);

        // Properties
        builder.Property(b => b.BookmarkId)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(b => b.AIResponseId)
            .IsRequired();

        builder.Property(b => b.BookmarkedOn)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(b => b.Notes)
            .HasMaxLength(1000);

        // Relationships

        // Many Bookmarks belong to one User
        builder.HasOne(b => b.User)
            .WithMany(u => u.Bookmarks)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many Bookmarks reference one AIResponse
        builder.HasOne(b => b.AIResponse)
            .WithMany(ar => ar.Bookmarks)
            .HasForeignKey(b => b.AIResponseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes

        // A user can only bookmark the same AI response once
        builder.HasIndex(b => new { b.UserId, b.AIResponseId })
            .IsUnique()
            .HasDatabaseName("IX_Bookmarks_UserId_AIResponseId");

        builder.HasIndex(b => b.UserId)
            .HasDatabaseName("IX_Bookmarks_UserId");
    }
}
