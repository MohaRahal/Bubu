using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Models;

namespace MyLifeApi.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(category => category.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(category => category.Description).HasColumnName("description").HasMaxLength(255);
        builder.Property(category => category.Icon).HasColumnName("icon").HasMaxLength(50);
        builder.Property(category => category.Color).HasColumnName("color").HasMaxLength(20);
        builder.Property(category => category.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(category => category.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(category => category.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(category => category.User)
            .WithMany(user => user.Categories)
            .HasForeignKey(category => category.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_categories_user");

        builder.HasIndex(category => category.UserId).HasDatabaseName("idx_categories_user");
        builder.HasIndex(category => new { category.UserId, category.Name }).IsUnique().HasDatabaseName("uq_categories_user_name");
    }
}
