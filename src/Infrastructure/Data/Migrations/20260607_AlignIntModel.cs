using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagementApp.Infrastructure.Data.Migrations
{
    public partial class AlignIntModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Placeholder migration: scaffold using dotnet-ef for precise schema changes if needed.
            // Example: If you changed any column types inadvertently, add alter column statements here.

            // migrationBuilder.AlterColumn<int>(
            //     name: "Id",
            //     table: "Routes",
            //     type: "INTEGER",
            //     nullable: false,
            //     oldClrType: typeof(Guid),
            //     oldType: "TEXT");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse operations of Up()
        }
    }
}
