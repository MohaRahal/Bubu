using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class Goal
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public GoalType GoalType { get; set; }
    public decimal? TargetValue { get; set; }
    public string? Unit { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<ActivityGoal> ActivityGoals { get; set; } = [];
}
