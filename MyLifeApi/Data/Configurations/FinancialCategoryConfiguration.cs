using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class FinancialCategoryConfiguration : IEntityTypeConfiguration<FinancialCategory>
{
    public void Configure(EntityTypeBuilder<FinancialCategory> builder)
    {
        builder.ToTable("financial_categories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(category => category.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(category => category.CategoryType).HasColumnName("category_type").HasConversion<SnakeCaseEnumConverter<TransactionType>>().HasColumnType("enum('income','expense')");
        builder.Property(category => category.Icon).HasColumnName("icon").HasMaxLength(50);
        builder.Property(category => category.Color).HasColumnName("color").HasMaxLength(20);
        builder.Property(category => category.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(category => category.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(category => category.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(category => category.User).WithMany(user => user.FinancialCategories).HasForeignKey(category => category.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_financial_categories_user");
        builder.HasIndex(category => category.UserId).HasDatabaseName("idx_financial_categories_user");
        builder.HasIndex(category => new { category.UserId, category.Name, category.CategoryType }).IsUnique().HasDatabaseName("uq_financial_category_user");
    }
}
