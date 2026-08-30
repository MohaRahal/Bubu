using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyLifeApi.Data.Converters;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Data.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("activities", table =>
        {
            table.HasCheckConstraint("chk_activity_dates", "ended_at IS NULL OR started_at IS NULL OR ended_at >= started_at");
            table.HasCheckConstraint("chk_activity_duration", "duration_seconds IS NULL OR duration_seconds >= 0");
        });
        builder.HasKey(activity => activity.Id);

        builder.Property(activity => activity.Id).HasColumnName("id").HasColumnType("bigint unsigned").ValueGeneratedOnAdd();
        builder.Property(activity => activity.UserId).HasColumnName("user_id").HasColumnType("bigint unsigned");
        builder.Property(activity => activity.CategoryId).HasColumnName("category_id").HasColumnType("bigint unsigned");
        builder.Property(activity => activity.Title).HasColumnName("title").HasMaxLength(150).IsRequired();
        builder.Property(activity => activity.Description).HasColumnName("description").HasColumnType("text");
        builder.Property(activity => activity.ActivityDate).HasColumnName("activity_date").HasColumnType("date");
        builder.Property(activity => activity.StartedAt).HasColumnName("started_at").HasColumnType("datetime");
        builder.Property(activity => activity.EndedAt).HasColumnName("ended_at").HasColumnType("datetime");
        builder.Property(activity => activity.DurationSeconds).HasColumnName("duration_seconds").HasColumnType("int unsigned");
        builder.Property(activity => activity.Status).HasColumnName("status").HasConversion<SnakeCaseEnumConverter<ActivityStatus>>().HasColumnType("enum('planned','in_progress','completed','cancelled')").HasDefaultValue(ActivityStatus.Completed).HasSentinel((ActivityStatus)(-1));
        builder.Property(activity => activity.Source).HasColumnName("source").HasConversion<SnakeCaseEnumConverter<ActivitySource>>().HasColumnType("enum('manual','timer','imported')").HasDefaultValue(ActivitySource.Manual);
        builder.Property(activity => activity.CreatedAt).HasColumnName("created_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
        builder.Property(activity => activity.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime").HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();

        builder.HasOne(activity => activity.User).WithMany(user => user.Activities).HasForeignKey(activity => activity.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_activities_user");
        builder.HasOne(activity => activity.Category).WithMany(category => category.Activities).HasForeignKey(activity => activity.CategoryId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("fk_activities_category");

        builder.HasIndex(activity => new { activity.UserId, activity.ActivityDate }).HasDatabaseName("idx_activities_user_date");
        builder.HasIndex(activity => new { activity.UserId, activity.StartedAt }).HasDatabaseName("idx_activities_user_started");
        builder.HasIndex(activity => new { activity.UserId, activity.CategoryId }).HasDatabaseName("idx_activities_user_category");
        builder.HasIndex(activity => new { activity.UserId, activity.Status }).HasDatabaseName("idx_activities_user_status");
    }
}
