using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLifeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetValidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "validated_at",
                table: "password_reset_requests",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "validation_token_hash",
                table: "password_reset_requests",
                type: "char(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "validated_at",
                table: "password_reset_requests");

            migrationBuilder.DropColumn(
                name: "validation_token_hash",
                table: "password_reset_requests");
        }
    }
}
