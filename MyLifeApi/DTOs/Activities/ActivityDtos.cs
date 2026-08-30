using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Activities;

public sealed record ActivityRequest(
    ulong UserId,
    ulong? CategoryId,
    [Required, StringLength(150)] string Title,
    string? Description,
    DateOnly ActivityDate,
    DateTime? StartedAt,
    DateTime? EndedAt,
    uint? DurationSeconds,
    ActivityStatus Status = ActivityStatus.Completed,
    ActivitySource Source = ActivitySource.Manual);

public sealed record ActivityResponse(
    ulong Id, ulong UserId, ulong? CategoryId, string Title,
    string? Description, DateOnly ActivityDate, DateTime? StartedAt,
    DateTime? EndedAt, uint? DurationSeconds, ActivityStatus Status,
    ActivitySource Source, DateTime CreatedAt, DateTime UpdatedAt);
