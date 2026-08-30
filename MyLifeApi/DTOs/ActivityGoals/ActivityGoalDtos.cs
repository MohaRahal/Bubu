namespace MyLifeApi.DTOs.ActivityGoals;

public sealed record CreateActivityGoalRequest(ulong ActivityId, ulong GoalId);

public sealed record ActivityGoalResponse(
    ulong ActivityId, ulong GoalId, DateTime CreatedAt);
