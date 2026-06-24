using EduAssist.EFCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.EFCore.Configurations;

/// <summary>
/// Fluent API configuration for the Category entity.
/// Defines table name, primary key, property constraints, and indexes.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Table name
        builder.ToTable("Categories");

        // Primary Key
        builder.HasKey(c => c.CategoryId);

        // Properties
        builder.Property(c => c.CategoryId)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.SubjectName)
            .IsRequired()
            .HasMaxLength(150);

        // Indexes - SubjectName should be unique
        builder.HasIndex(c => c.SubjectName)
            .IsUnique()
            .HasDatabaseName("IX_Categories_SubjectName");
    }
}
