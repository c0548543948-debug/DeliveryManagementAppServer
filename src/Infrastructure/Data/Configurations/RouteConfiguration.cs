using DeliveryManagementApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryManagementApp.Infrastructure.Data.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Date).IsRequired();
        builder.Property(r => r.StartTime).IsRequired();
        builder.Property(r => r.EndTime).IsRequired();

        builder.HasOne(r => r.Courier)
            .WithMany(c => c.Routes)
            .HasForeignKey(r => r.CourierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.Routes)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
