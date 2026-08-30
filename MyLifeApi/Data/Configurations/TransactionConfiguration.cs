using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", table =>
            table.HasCheckConstraint("chk_transaction_amount", "amount > 0"));
        builder.HasKey(transaction => transaction.Id);

        builder.Property(transaction => transaction.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(transaction => transaction.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(transaction => transaction.AccountId).HasColumnName("account_id").HasColumnType("bigint unsigned");
        builder.Property(transaction => transaction.CategoryId).HasColumnName("category_id").HasColumnType("bigint unsigned");
        builder.Property(transaction => transaction.RecurringTransactionId).HasColumnName("recurring_transaction_id").HasColumnType("bigint unsigned");
        builder.Property(transaction => transaction.TransactionType).HasColumnName("transaction_type").HasConversion<SnakeCaseEnumConverter<TransactionType>>().HasColumnType("enum('income','expense')");
        builder.Property(transaction => transaction.Description).HasColumnName("description").HasMaxLength(200).IsRequired();
        builder.Property(transaction => transaction.Amount).HasColumnName("amount").HasPrecision(14, 2);
        builder.Property(transaction => transaction.TransactionDate).HasColumnName("transaction_date").HasColumnType("date");
        builder.Property(transaction => transaction.Status).HasColumnName("status").HasConversion<SnakeCaseEnumConverter<TransactionStatus>>().HasColumnType("enum('pending','completed','cancelled')").HasDefaultValue(TransactionStatus.Completed).HasSentinel((TransactionStatus)(-1));
        builder.Property(transaction => transaction.Notes).HasColumnName("notes").HasColumnType("text");
        builder.Property(transaction => transaction.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(transaction => transaction.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(transaction => transaction.User).WithMany(user => user.Transactions).HasForeignKey(transaction => transaction.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_transactions_user");
        builder.HasOne(transaction => transaction.Account).WithMany(account => account.Transactions).HasForeignKey(transaction => transaction.AccountId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_transactions_account");
        builder.HasOne(transaction => transaction.Category).WithMany(category => category.Transactions).HasForeignKey(transaction => transaction.CategoryId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("fk_transactions_category");
        builder.HasOne(transaction => transaction.RecurringTransaction).WithMany(recurring => recurring.Transactions).HasForeignKey(transaction => transaction.RecurringTransactionId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("fk_transactions_recurring");

        builder.HasIndex(transaction => new { transaction.UserId, transaction.TransactionDate }).HasDatabaseName("idx_transactions_user_date");
        builder.HasIndex(transaction => new { transaction.AccountId, transaction.TransactionDate }).HasDatabaseName("idx_transactions_account_date");
        builder.HasIndex(transaction => transaction.CategoryId).HasDatabaseName("idx_transactions_category");
        builder.HasIndex(transaction => transaction.RecurringTransactionId).HasDatabaseName("idx_transactions_recurring");
    }
}
