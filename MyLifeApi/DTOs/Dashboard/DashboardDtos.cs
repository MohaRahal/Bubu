using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Dashboard;

public sealed record DashboardResponse(
    DashboardSummaryResponse Summary,
    IReadOnlyList<DashboardChartPointResponse> LastSevenDays,
    IReadOnlyList<DashboardActivityResponse> UpcomingActivities,
    IReadOnlyList<DashboardGoalResponse> ActiveGoals,
    IReadOnlyList<DashboardTransactionResponse> RecentTransactions,
    string Insight);

public sealed record DashboardSummaryResponse(
    int CompletedActivitiesToday,
    int TotalActivitiesToday,
    long FocusSecondsToday,
    int ActiveGoals,
    decimal TotalBalance,
    decimal MonthIncome,
    decimal MonthExpense);

public sealed record DashboardChartPointResponse(
    DateOnly Date,
    int CompletedActivities,
    long FocusSeconds,
    decimal Income,
    decimal Expense);

public sealed record DashboardActivityResponse(
    ulong Id,
    string Title,
    DateOnly ActivityDate,
    DateTime? StartedAt,
    ActivityStatus Status,
    string? CategoryName,
    string? CategoryColor);

public sealed record DashboardGoalResponse(
    ulong Id,
    string Title,
    DateOnly? TargetDate,
    decimal? TargetValue,
    string? Unit,
    GoalType GoalType,
    decimal CurrentValue,
    int CompletedActivities,
    int TotalActivities,
    decimal ProgressPercentage);

public sealed record DashboardTransactionResponse(
    ulong Id,
    string Description,
    decimal Amount,
    DateOnly TransactionDate,
    TransactionType TransactionType,
    string AccountName,
    string? CategoryName);
