namespace MyLifeApi.Models;

public class ActivityGoal
{
    public ulong ActivityId { get; set; }
    public ulong GoalId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Activity Activity { get; set; } = null!;
    public Goal Goal { get; set; } = null!;
}
