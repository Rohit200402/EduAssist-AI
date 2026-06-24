using EduAssist.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.EFCore.Configurations;

/// <summary>
/// Fluent API configuration for the AIResponse entity.
/// Defines constraints, relationships, and indexes.
/// </summary>
public class AIResponseConfiguration : IEntityTypeConfiguration<AIResponse>
{
    public void Configure(EntityTypeBuilder<AIResponse> builder)
    {
        // Table name
        builder.ToTable("AIResponses");

        // Primary Key
        builder.HasKey(ar => ar.AIResponseId);

        // Properties
        builder.Property(ar => ar.AIResponseId)
            .ValueGeneratedOnAdd();

        builder.Property(ar => ar.Response)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(ar => ar.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(ar => ar.UserRequestId)
            .IsRequired();

        // Relationship: Many AIResponses belong to one UserRequest
        builder.HasOne(ar => ar.UserRequest)
            .WithMany(ur => ur.AIResponses)
            .HasForeignKey(ar => ar.UserRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(ar => ar.UserRequestId)
            .HasDatabaseName("IX_AIResponses_UserRequestId");

        builder.HasIndex(ar => ar.CreatedAt)
            .HasDatabaseName("IX_AIResponses_CreatedAt");
    }
}
