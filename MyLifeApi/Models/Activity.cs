using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class Activity
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong? CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly ActivityDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public uint? DurationSeconds { get; set; }
    public ActivityStatus Status { get; set; } = ActivityStatus.Completed;
    public ActivitySource Source { get; set; } = ActivitySource.Manual;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Category? Category { get; set; }
    public ICollection<ActivityGoal> ActivityGoals { get; set; } = [];
}
