using EduAssist.AngularWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.AngularWebApi.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.CategoryId);
        builder.Property(c => c.CategoryId).ValueGeneratedOnAdd();
        builder.Property(c => c.SubjectName).IsRequired().HasMaxLength(150);
        builder.HasIndex(c => c.SubjectName).IsUnique();
    }
}
