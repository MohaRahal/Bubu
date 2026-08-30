using System.ComponentModel.DataAnnotations;

namespace MyLifeApi.DTOs.MyDay;

public sealed record MyDayResponse(
    DateOnly Date,
    MyDaySummaryResponse Summary,
    IReadOnlyList<MyDayActivityResponse> Activities,
    IReadOnlyList<MyDayCategoryResponse> Categories);

public sealed record MyDaySummaryResponse(
    int Total,
    int Completed,
    int Planned,
    int InProgress,
    uint FocusSeconds,
    decimal CompletionPercentage);

public sealed record MyDayActivityResponse(
    ulong Id,
    string Title,
    string? Description,
    DateOnly ActivityDate,
    DateTime? StartedAt,
    DateTime? EndedAt,
    uint? DurationSeconds,
    string Status,
    bool IsPaused,
    string? CategoryName,
    string? CategoryColor);

public sealed record MyDayCategoryResponse(
    ulong Id,
    string Name,
    string? Color,
    string? Icon);

public sealed record CreateMyDayActivityRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Title,
    [StringLength(1000)] string? Description,
    DateOnly ActivityDate,
    TimeOnly? StartTime,
    [Range(1, 1440)] int? DurationMinutes,
    ulong? CategoryId);
