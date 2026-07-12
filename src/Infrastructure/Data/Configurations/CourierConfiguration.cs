using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryManagementApp.Infrastructure.Data.Configurations;

public class CourierConfiguration : IEntityTypeConfiguration<Courier>
{
    public void Configure(EntityTypeBuilder<Courier> builder)
    {
        builder.Property(c => c.ApplicationUserId).IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Courier>(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
