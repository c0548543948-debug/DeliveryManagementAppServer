using DeliveryManagementApp.Domain.Entities;
using DeliveryManagementApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryManagementApp.Infrastructure.Data.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.OldStatus)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.NewStatus)
            .HasConversion<int>()
            .IsRequired();

        builder
            .OwnsOne(a => a.LocationData);

        builder.HasIndex(a => a.OrderId);

        builder.HasOne(a => a.Order)
            .WithMany(o => o.ActivityLogs)
            .HasForeignKey(a => a.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
