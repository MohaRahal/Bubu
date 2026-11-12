using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public sealed class GoalContributionConfiguration : IEntityTypeConfiguration<GoalContribution>
{
    public void Configure(EntityTypeBuilder<GoalContribution> builder)
    {
        builder.ToTable("goal_contributions", table =>
            table.HasCheckConstraint("chk_goal_contribution_amount", "amount > 0"));
        builder.HasKey(contribution => contribution.Id);

        builder.Property(contribution => contribution.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .ValueGeneratedOnAdd();
        builder.Property(contribution => contribution.GoalId)
            .HasColumnName("goal_id")
            .HasColumnType("bigint unsigned");
        builder.Property(contribution => contribution.AccountId)
            .HasColumnName("account_id")
            .HasColumnType("bigint unsigned");
        builder.Property(contribution => contribution.ContributionType)
            .HasColumnName("contribution_type")
            .HasConversion<SnakeCaseEnumConverter<GoalContributionType>>()
            .HasColumnType("enum('deposit','withdrawal')");
        builder.Property(contribution => contribution.Amount)
            .HasColumnName("amount")
            .HasPrecision(12, 2);
        builder.Property(contribution => contribution.Note)
            .HasColumnName("note")
            .HasMaxLength(300);
        builder.Property(contribution => contribution.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasOne(contribution => contribution.Goal)
            .WithMany(goal => goal.Contributions)
            .HasForeignKey(contribution => contribution.GoalId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_goal_contributions_goal");

        builder.HasOne(contribution => contribution.Account)
            .WithMany(account => account.GoalContributions)
            .HasForeignKey(contribution => contribution.AccountId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_goal_contributions_account");

        builder.HasIndex(contribution => new { contribution.GoalId, contribution.CreatedAt })
            .HasDatabaseName("idx_goal_contributions_goal_created");
        builder.HasIndex(contribution => new { contribution.AccountId, contribution.CreatedAt })
            .HasDatabaseName("idx_goal_contributions_account_created");
    }
}
