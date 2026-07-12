using DeliveryManagementApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryManagementApp.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.LicensePlate)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(v => v.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(v => v.CapacityVolume)
            .IsRequired();

        builder.Property(v => v.CapacityWeight)
            .IsRequired();
    }
}
