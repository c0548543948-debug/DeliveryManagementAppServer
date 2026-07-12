using DeliveryManagementApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryManagementApp.Infrastructure.Data.Configurations;

public class RouteItemConfiguration : IEntityTypeConfiguration<RouteItem>
{
    public void Configure(EntityTypeBuilder<RouteItem> builder)
    {
        builder.HasKey(ri => ri.Id);

        builder.Property(ri => ri.StopOrder).IsRequired();
        builder.Property(ri => ri.StopType).HasConversion<int>().IsRequired();

        builder.HasIndex(ri => new { ri.RouteId, ri.StopOrder }).IsUnique();

        builder.HasOne(ri => ri.Route)
            .WithMany(r => r.Items)
            .HasForeignKey(ri => ri.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ri => ri.Order)
            .WithMany(o => o.RouteItems)
            .HasForeignKey(ri => ri.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
