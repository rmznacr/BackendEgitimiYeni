using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendEgitimiYeni.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageNumber",
                table: "DocumentChunks",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PageNumber",
                table: "DocumentChunks");
        }
    }
}
