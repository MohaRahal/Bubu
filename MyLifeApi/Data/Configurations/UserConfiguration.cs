using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();

        builder.Property(user => user.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("email");

        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(user => user.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        builder.Property(user => user.BirthDate)
            .HasColumnName("birth_date")
            .HasColumnType("date");

        builder.Property(user => user.Status)
            .HasColumnName("status")
            .HasConversion<SnakeCaseEnumConverter<UserStatus>>()
            .HasColumnType("enum('active','inactive','blocked')")
            .HasDefaultValue(UserStatus.Active)
            .IsRequired();

        builder.Property(user => user.LastLoginAt)
            .HasColumnName("last_login_at")
            .HasColumnType("datetime");

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.Property(user => user.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
            .ValueGeneratedOnAddOrUpdate();
    }
}
