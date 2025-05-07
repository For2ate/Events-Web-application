using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_props_event : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentNumberOfParticipants",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Place",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentNumberOfParticipants",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Place",
                table: "Events");
        }
    }
}
