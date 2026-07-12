using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryManagementApp.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.Property(c => c.ApplicationUserId).IsRequired();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Customer>(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
