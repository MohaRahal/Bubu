using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals", table =>
        {
            table.HasCheckConstraint("chk_goal_dates", "target_date IS NULL OR start_date IS NULL OR target_date >= start_date");
            table.HasCheckConstraint("chk_goal_target", "target_value IS NULL OR target_value >= 0");
        });
        builder.HasKey(goal => goal.Id);

        builder.Property(goal => goal.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(goal => goal.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(goal => goal.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
        builder.Property(goal => goal.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(goal => goal.GoalType).HasColumnName("goal_type").HasConversion<SnakeCaseEnumConverter<GoalType>>().HasColumnType("enum('duration','quantity','money','boolean')");
        builder.Property(goal => goal.TargetValue).HasColumnName("target_value").HasPrecision(12, 2);
        builder.Property(goal => goal.Unit).HasColumnName("unit").HasMaxLength(30);
        builder.Property(goal => goal.StartDate).HasColumnName("start_date").HasColumnType("date");
        builder.Property(goal => goal.TargetDate).HasColumnName("target_date").HasColumnType("date");
        builder.Property(goal => goal.Status).HasColumnName("status").HasConversion<SnakeCaseEnumConverter<GoalStatus>>().HasColumnType("enum('active','completed','paused','cancelled')").HasDefaultValue(GoalStatus.Active);
        builder.Property(goal => goal.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(goal => goal.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(goal => goal.User).WithMany(user => user.Goals).HasForeignKey(goal => goal.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_goals_user");
        builder.HasIndex(goal => new { goal.UserId, goal.Status }).HasDatabaseName("idx_goals_user_status");
        builder.HasIndex(goal => new { goal.UserId, goal.TargetDate }).HasDatabaseName("idx_goals_user_target_date");
    }
}
