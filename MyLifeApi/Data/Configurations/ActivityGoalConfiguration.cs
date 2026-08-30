using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Models;

namespace MyLifeApi.Data.Configurations;

public class ActivityGoalConfiguration : IEntityTypeConfiguration<ActivityGoal>
{
    public void Configure(EntityTypeBuilder<ActivityGoal> builder)
    {
        builder.ToTable("activity_goals");
        builder.HasKey(link => new { link.ActivityId, link.GoalId });

        builder.Property(link => link.ActivityId).HasColumnName("activity_id").HasColumnType("bigint unsigned");
        builder.Property(link => link.GoalId).HasColumnName("goal_id").HasColumnType("bigint unsigned");
        builder.Property(link => link.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();

        builder.HasOne(link => link.Activity).WithMany(activity => activity.ActivityGoals).HasForeignKey(link => link.ActivityId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_activity_goals_activity");
        builder.HasOne(link => link.Goal).WithMany(goal => goal.ActivityGoals).HasForeignKey(link => link.GoalId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_activity_goals_goal");
    }
}
