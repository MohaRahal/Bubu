using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLifeApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTransactionUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "uq_transactions_recurring_date",
                table: "transactions",
                columns: new[] { "recurring_transaction_id", "transaction_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "uq_transactions_recurring_date",
                table: "transactions");
        }
    }
}
