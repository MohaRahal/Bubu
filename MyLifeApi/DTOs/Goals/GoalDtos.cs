using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Goals;

public sealed record GoalRequest(
    ulong UserId,
    [Required, StringLength(150)] string Title,
    string? Description,
    GoalType GoalType,
    [Range(typeof(decimal), "0", "9999999999.99", ParseLimitsInInvariantCulture = true)] decimal? TargetValue,
    [StringLength(30)] string? Unit,
    DateOnly? StartDate,
    DateOnly? TargetDate,
    GoalStatus Status = GoalStatus.Active);

public sealed record GoalResponse(
    ulong Id, ulong UserId, string Title, string? Description,
    GoalType GoalType, decimal? TargetValue, string? Unit,
    DateOnly? StartDate, DateOnly? TargetDate, GoalStatus Status,
    DateTime CreatedAt, DateTime UpdatedAt);
