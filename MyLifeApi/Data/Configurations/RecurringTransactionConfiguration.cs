using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
{
    public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
    {
        builder.ToTable("recurring_transactions", table =>
        {
            table.HasCheckConstraint("chk_recurring_amount", "amount > 0");
            table.HasCheckConstraint("chk_recurring_dates", "end_date IS NULL OR end_date >= start_date");
            table.HasCheckConstraint("chk_recurring_next_due", "next_due_date >= start_date");
        });
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(transaction => transaction.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(transaction => transaction.AccountId).HasColumnName("account_id").HasColumnType("bigint unsigned");
        builder.Property(transaction => transaction.CategoryId).HasColumnName("category_id").HasColumnType("bigint unsigned");
        builder.Property(transaction => transaction.TransactionType).HasColumnName("transaction_type").HasConversion<SnakeCaseEnumConverter<TransactionType>>().HasColumnType("enum('income','expense')");
        builder.Property(transaction => transaction.Description).HasColumnName("description").HasMaxLength(200).IsRequired();
        builder.Property(transaction => transaction.Amount).HasColumnName("amount").HasPrecision(14, 2);
        builder.Property(transaction => transaction.Frequency).HasColumnName("frequency").HasConversion<SnakeCaseEnumConverter<RecurrenceFrequency>>().HasColumnType("enum('daily','weekly','monthly','yearly')");
        builder.Property(transaction => transaction.StartDate).HasColumnName("start_date").HasColumnType("date");
        builder.Property(transaction => transaction.EndDate).HasColumnName("end_date").HasColumnType("date");
        builder.Property(transaction => transaction.NextDueDate).HasColumnName("next_due_date").HasColumnType("date");
        builder.Property(transaction => transaction.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(transaction => transaction.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(transaction => transaction.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(transaction => transaction.User).WithMany(user => user.RecurringTransactions).HasForeignKey(transaction => transaction.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_recurring_user");
        builder.HasOne(transaction => transaction.Account).WithMany(account => account.RecurringTransactions).HasForeignKey(transaction => transaction.AccountId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_recurring_account");
        builder.HasOne(transaction => transaction.Category).WithMany(category => category.RecurringTransactions).HasForeignKey(transaction => transaction.CategoryId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("fk_recurring_category");

        builder.HasIndex(transaction => new { transaction.UserId, transaction.NextDueDate }).HasDatabaseName("idx_recurring_user_due");
        builder.HasIndex(transaction => new { transaction.IsActive, transaction.NextDueDate }).HasDatabaseName("idx_recurring_active_due");
    }
}
