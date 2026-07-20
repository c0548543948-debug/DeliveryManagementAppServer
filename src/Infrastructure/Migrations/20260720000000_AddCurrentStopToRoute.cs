using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryManagementApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentStopToRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentStop",
                table: "Routes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStop",
                table: "Routes");
        }
    }
}
