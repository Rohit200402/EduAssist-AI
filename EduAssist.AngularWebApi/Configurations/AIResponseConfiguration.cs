using EduAssist.AngularWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.AngularWebApi.Configurations;

public class AIResponseConfiguration : IEntityTypeConfiguration<AIResponse>
{
    public void Configure(EntityTypeBuilder<AIResponse> builder)
    {
        builder.ToTable("AIResponses");
        builder.HasKey(ar => ar.AIResponseId);
        builder.Property(ar => ar.AIResponseId).ValueGeneratedOnAdd();
        builder.Property(ar => ar.Response).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(ar => ar.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(ar => ar.UserRequest)
            .WithMany(ur => ur.AIResponses)
            .HasForeignKey(ar => ar.UserRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ar => ar.UserRequestId);
    }
}
