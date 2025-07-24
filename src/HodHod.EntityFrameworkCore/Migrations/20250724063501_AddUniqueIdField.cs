using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HodHod.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIdField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueId",
                table: "AppReports",
                type: "nvarchar(12)",
                maxLength: 12,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueId",
                table: "AppReports");
        }
    }
}
