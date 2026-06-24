using EduAssist.AngularWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.AngularWebApi.Configurations;

public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.ToTable("Bookmarks");
        builder.HasKey(b => b.BookmarkId);
        builder.Property(b => b.BookmarkId).ValueGeneratedOnAdd();
        builder.Property(b => b.UserId).IsRequired().HasMaxLength(450);
        builder.Property(b => b.BookmarkedOn).IsRequired().HasDefaultValueSql("GETUTCDATE()");
        builder.Property(b => b.Notes).HasMaxLength(1000);

        builder.HasOne(b => b.AIResponse)
            .WithMany(ar => ar.Bookmarks)
            .HasForeignKey(b => b.AIResponseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => new { b.UserId, b.AIResponseId }).IsUnique();
        builder.HasIndex(b => b.UserId);
    }
}
