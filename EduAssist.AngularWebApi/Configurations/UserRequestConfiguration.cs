using EduAssist.AngularWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduAssist.AngularWebApi.Configurations;

public class UserRequestConfiguration : IEntityTypeConfiguration<UserRequest>
{
    public void Configure(EntityTypeBuilder<UserRequest> builder)
    {
        builder.ToTable("UserRequests");
        builder.HasKey(ur => ur.UserRequestId);
        builder.Property(ur => ur.UserRequestId).ValueGeneratedOnAdd();
        builder.Property(ur => ur.Query).IsRequired().HasMaxLength(2000);
        builder.Property(ur => ur.UserId).IsRequired().HasMaxLength(450);
        builder.Property(ur => ur.RequestedOn).IsRequired().HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(ur => ur.Category)
            .WithMany(c => c.UserRequests)
            .HasForeignKey(ur => ur.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ur => ur.AIResponses)
            .WithOne(ar => ar.UserRequest)
            .HasForeignKey(ar => ar.UserRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ur => ur.UserId);
        builder.HasIndex(ur => ur.CategoryId);
        builder.HasIndex(ur => ur.RequestedOn);
    }
}
