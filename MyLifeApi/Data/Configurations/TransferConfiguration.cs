using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers", table =>
        {
            table.HasCheckConstraint("chk_transfer_amount", "amount > 0");
            table.HasCheckConstraint("chk_transfer_accounts", "from_account_id <> to_account_id");
        });
        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(transfer => transfer.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(transfer => transfer.FromAccountId).HasColumnName("from_account_id").HasColumnType("bigint unsigned");
        builder.Property(transfer => transfer.ToAccountId).HasColumnName("to_account_id").HasColumnType("bigint unsigned");
        builder.Property(transfer => transfer.Amount).HasColumnName("amount").HasPrecision(14, 2);
        builder.Property(transfer => transfer.TransferDate).HasColumnName("transfer_date").HasColumnType("datetime");
        builder.Property(transfer => transfer.Description).HasColumnName("description").HasMaxLength(200);
        builder.Property(transfer => transfer.Status).HasColumnName("status").HasConversion<SnakeCaseEnumConverter<TransactionStatus>>().HasColumnType("enum('pending','completed','cancelled')").HasDefaultValue(TransactionStatus.Completed).HasSentinel((TransactionStatus)(-1));
        builder.Property(transfer => transfer.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(transfer => transfer.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(transfer => transfer.User).WithMany(user => user.Transfers).HasForeignKey(transfer => transfer.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_transfers_user");
        builder.HasOne(transfer => transfer.FromAccount).WithMany(account => account.OutgoingTransfers).HasForeignKey(transfer => transfer.FromAccountId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_transfers_from_account");
        builder.HasOne(transfer => transfer.ToAccount).WithMany(account => account.IncomingTransfers).HasForeignKey(transfer => transfer.ToAccountId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_transfers_to_account");

        builder.HasIndex(transfer => new { transfer.UserId, transfer.TransferDate }).HasDatabaseName("idx_transfers_user_date");
    }
}
