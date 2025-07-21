using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HodHod.Migrations
{
    public partial class AddPublicIdToCategoryAndSubCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "AppCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "AppSubCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "AppSubCategories");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "AppCategories");
        }
    }
}