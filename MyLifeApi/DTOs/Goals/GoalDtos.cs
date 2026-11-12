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

public sealed record GoalVaultRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Title,
    [StringLength(1000)] string? Description,
    [Range(typeof(decimal), "0.01", "9999999999.99", ParseLimitsInInvariantCulture = true)] decimal TargetValue,
    DateOnly? TargetDate);

public sealed record GoalContributionRequest(
    GoalContributionType ContributionType,
    ulong? AccountId,
    [Range(typeof(decimal), "0.01", "9999999999.99", ParseLimitsInInvariantCulture = true)] decimal Amount,
    [StringLength(300)] string? Note);

public sealed record GoalContributionResponse(
    ulong Id,
    GoalContributionType ContributionType,
    ulong? AccountId,
    string? AccountName,
    decimal Amount,
    string? Note,
    DateTime CreatedAt);

public sealed record GoalAccountOptionResponse(
    ulong Id,
    string Name,
    decimal Balance);

public sealed record GoalVaultResponse(
    ulong Id,
    string Title,
    string? Description,
    GoalType GoalType,
    string? Unit,
    decimal TargetValue,
    decimal CurrentValue,
    decimal ProgressPercentage,
    DateOnly? TargetDate,
    GoalStatus Status,
    IReadOnlyList<GoalContributionResponse> RecentContributions,
    DateTime CreatedAt);
