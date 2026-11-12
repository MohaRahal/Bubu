using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Models;

namespace MyLifeApi.Data.Configurations;

public sealed class PasswordResetRequestConfiguration
    : IEntityTypeConfiguration<PasswordResetRequest>
{
    public void Configure(EntityTypeBuilder<PasswordResetRequest> builder)
    {
        builder.ToTable("password_reset_requests");
        builder.HasKey(request => request.Id);

        builder.Property(request => request.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        builder.Property(request => request.UserId)
            .HasColumnName("user_id")
            .HasColumnType("bigint unsigned");
        builder.Property(request => request.RequestedEmail)
            .HasColumnName("requested_email")
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(request => request.CodeHash)
            .HasColumnName("code_hash")
            .HasMaxLength(64)
            .IsFixedLength()
            .IsRequired();
        builder.Property(request => request.CodeSalt)
            .HasColumnName("code_salt")
            .HasMaxLength(24)
            .IsRequired();
        builder.Property(request => request.RequestedIp)
            .HasColumnName("requested_ip")
            .HasMaxLength(45);
        builder.Property(request => request.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(512);
        builder.Property(request => request.FailedAttempts)
            .HasColumnName("failed_attempts")
            .HasColumnType("int unsigned")
            .HasDefaultValue(0);
        builder.Property(request => request.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("datetime")
            .IsRequired();
        builder.Property(request => request.ValidationTokenHash)
            .HasColumnName("validation_token_hash")
            .HasMaxLength(64)
            .IsFixedLength();
        builder.Property(request => request.ValidatedAt)
            .HasColumnName("validated_at")
            .HasColumnType("datetime");
        builder.Property(request => request.UsedAt)
            .HasColumnName("used_at")
            .HasColumnType("datetime");
        builder.Property(request => request.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("datetime");
        builder.Property(request => request.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasOne(request => request.User)
            .WithMany(user => user.PasswordResetRequests)
            .HasForeignKey(request => request.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_password_reset_requests_user");

        builder.HasIndex(request => new { request.RequestedEmail, request.CreatedAt })
            .HasDatabaseName("idx_password_reset_requests_email_created");
        builder.HasIndex(request => new { request.UserId, request.CreatedAt })
            .HasDatabaseName("idx_password_reset_requests_user_created");
        builder.HasIndex(request => request.ExpiresAt)
            .HasDatabaseName("idx_password_reset_requests_expires_at");
    }
}
