using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLifeApi.Migrations
{
    /// <inheritdoc />
    public partial class created_database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "enum('active','inactive','blocked')", nullable: false, defaultValue: "active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_login_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    account_type = table.Column<string>(type: "enum('checking','savings','cash','wallet','investment','other')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    initial_balance = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false, defaultValue: 0.00m),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_accounts_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "financial_categories",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category_type = table.Column<string>(type: "enum('income','expense')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    color = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_financial_categories_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    goal_type = table.Column<string>(type: "enum('duration','quantity','money','boolean')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    target_value = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    unit = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    target_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "enum('active','completed','paused','cancelled')", nullable: false, defaultValue: "active")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.id);
                    table.CheckConstraint("chk_goal_dates", "target_date IS NULL OR start_date IS NULL OR target_date >= start_date");
                    table.CheckConstraint("chk_goal_target", "target_value IS NULL OR target_value >= 0");
                    table.ForeignKey(
                        name: "fk_goals_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transfers",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    from_account_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    to_account_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    transfer_date = table.Column<DateTime>(type: "datetime", nullable: false),
                    description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "enum('pending','completed','cancelled')", nullable: false, defaultValue: "completed")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfers", x => x.id);
                    table.CheckConstraint("chk_transfer_accounts", "from_account_id <> to_account_id");
                    table.CheckConstraint("chk_transfer_amount", "amount > 0");
                    table.ForeignKey(
                        name: "fk_transfers_from_account",
                        column: x => x.from_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transfers_to_account",
                        column: x => x.to_account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transfers_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    category_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activity_date = table.Column<DateOnly>(type: "date", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    ended_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    duration_seconds = table.Column<uint>(type: "int unsigned", nullable: true),
                    status = table.Column<string>(type: "enum('planned','in_progress','completed','cancelled')", nullable: false, defaultValue: "completed")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    source = table.Column<string>(type: "enum('manual','timer','imported')", nullable: false, defaultValue: "manual")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.id);
                    table.CheckConstraint("chk_activity_dates", "ended_at IS NULL OR started_at IS NULL OR ended_at >= started_at");
                    table.CheckConstraint("chk_activity_duration", "duration_seconds IS NULL OR duration_seconds >= 0");
                    table.ForeignKey(
                        name: "fk_activities_category",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_activities_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "recurring_transactions",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    account_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    category_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    transaction_type = table.Column<string>(type: "enum('income','expense')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    frequency = table.Column<string>(type: "enum('daily','weekly','monthly','yearly')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    next_due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurring_transactions", x => x.id);
                    table.CheckConstraint("chk_recurring_amount", "amount > 0");
                    table.CheckConstraint("chk_recurring_dates", "end_date IS NULL OR end_date >= start_date");
                    table.CheckConstraint("chk_recurring_next_due", "next_due_date >= start_date");
                    table.ForeignKey(
                        name: "fk_recurring_account",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_recurring_category",
                        column: x => x.category_id,
                        principalTable: "financial_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_recurring_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "activity_goals",
                columns: table => new
                {
                    activity_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    goal_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_goals", x => new { x.activity_id, x.goal_id });
                    table.ForeignKey(
                        name: "fk_activity_goals_activity",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_goals_goal",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    account_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    category_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    recurring_transaction_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    transaction_type = table.Column<string>(type: "enum('income','expense')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    amount = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: false),
                    transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "enum('pending','completed','cancelled')", nullable: false, defaultValue: "completed")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.CheckConstraint("chk_transaction_amount", "amount > 0");
                    table.ForeignKey(
                        name: "fk_transactions_account",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transactions_category",
                        column: x => x.category_id,
                        principalTable: "financial_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_transactions_recurring",
                        column: x => x.recurring_transaction_id,
                        principalTable: "recurring_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_transactions_user",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_accounts_user",
                table: "accounts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "idx_activities_user_category",
                table: "activities",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "idx_activities_user_date",
                table: "activities",
                columns: new[] { "user_id", "activity_date" });

            migrationBuilder.CreateIndex(
                name: "idx_activities_user_started",
                table: "activities",
                columns: new[] { "user_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "idx_activities_user_status",
                table: "activities",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_activities_category_id",
                table: "activities",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_goals_goal_id",
                table: "activity_goals",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "idx_categories_user",
                table: "categories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_categories_user_name",
                table: "categories",
                columns: new[] { "user_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_financial_categories_user",
                table: "financial_categories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_financial_category_user",
                table: "financial_categories",
                columns: new[] { "user_id", "name", "category_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_goals_user_status",
                table: "goals",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_goals_user_target_date",
                table: "goals",
                columns: new[] { "user_id", "target_date" });

            migrationBuilder.CreateIndex(
                name: "idx_recurring_active_due",
                table: "recurring_transactions",
                columns: new[] { "is_active", "next_due_date" });

            migrationBuilder.CreateIndex(
                name: "idx_recurring_user_due",
                table: "recurring_transactions",
                columns: new[] { "user_id", "next_due_date" });

            migrationBuilder.CreateIndex(
                name: "IX_recurring_transactions_account_id",
                table: "recurring_transactions",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_recurring_transactions_category_id",
                table: "recurring_transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_account_date",
                table: "transactions",
                columns: new[] { "account_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "idx_transactions_category",
                table: "transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_recurring",
                table: "transactions",
                column: "recurring_transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_transactions_user_date",
                table: "transactions",
                columns: new[] { "user_id", "transaction_date" });

            migrationBuilder.CreateIndex(
                name: "idx_transfers_user_date",
                table: "transfers",
                columns: new[] { "user_id", "transfer_date" });

            migrationBuilder.CreateIndex(
                name: "IX_transfers_from_account_id",
                table: "transfers",
                column: "from_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_transfers_to_account_id",
                table: "transfers",
                column: "to_account_id");

            migrationBuilder.CreateIndex(
                name: "email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_goals");

            migrationBuilder.DropTable(
                name: "transactions");

            migrationBuilder.DropTable(
                name: "transfers");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "goals");

            migrationBuilder.DropTable(
                name: "recurring_transactions");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "financial_categories");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
