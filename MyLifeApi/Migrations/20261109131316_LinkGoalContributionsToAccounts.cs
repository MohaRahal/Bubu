using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLifeApi.Migrations
{
    /// <inheritdoc />
    public partial class LinkGoalContributionsToAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "account_id",
                table: "goal_contributions",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_goal_contributions_account_created",
                table: "goal_contributions",
                columns: new[] { "account_id", "created_at" });

            migrationBuilder.AddForeignKey(
                name: "fk_goal_contributions_account",
                table: "goal_contributions",
                column: "account_id",
                principalTable: "accounts",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_goal_contributions_account",
                table: "goal_contributions");

            migrationBuilder.DropIndex(
                name: "idx_goal_contributions_account_created",
                table: "goal_contributions");

            migrationBuilder.DropColumn(
                name: "account_id",
                table: "goal_contributions");
        }
    }
}
