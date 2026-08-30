using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(account => account.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(account => account.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(account => account.AccountType).HasColumnName("account_type").HasConversion<SnakeCaseEnumConverter<AccountType>>().HasColumnType("enum('checking','savings','cash','wallet','investment','other')");
        builder.Property(account => account.InitialBalance).HasColumnName("initial_balance").HasPrecision(14, 2).HasDefaultValue(0.00m);
        builder.Property(account => account.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(account => account.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(account => account.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(account => account.User).WithMany(user => user.Accounts).HasForeignKey(account => account.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_accounts_user");
        builder.HasIndex(account => account.UserId).HasDatabaseName("idx_accounts_user");
    }
}
