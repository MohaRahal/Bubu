using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.QuickRegister;

public sealed record QuickRegisterOptionsResponse(
    IReadOnlyList<QuickAccountResponse> Accounts,
    IReadOnlyList<QuickFinancialCategoryResponse> FinancialCategories,
    IReadOnlyList<QuickActivityCategoryResponse> ActivityCategories);

public sealed record QuickAccountResponse(ulong Id, string Name);

public sealed record QuickFinancialCategoryResponse(
    ulong Id,
    string Name,
    TransactionType Type,
    string? Color);

public sealed record QuickActivityCategoryResponse(
    ulong Id,
    string Name,
    string? Color);

public sealed record QuickGoalRequest(
    [Required, StringLength(150, MinimumLength = 2)] string Title,
    [StringLength(1000)] string? Description,
    GoalType GoalType,
    [Range(typeof(decimal), "0.01", "9999999999.99", ParseLimitsInInvariantCulture = true)] decimal? TargetValue,
    [StringLength(30)] string? Unit,
    DateOnly StartDate,
    DateOnly? TargetDate);

public sealed record QuickTransactionRequest(
    TransactionType TransactionType,
    ulong AccountId,
    ulong? CategoryId,
    [Required, StringLength(200, MinimumLength = 2)] string Description,
    [Range(typeof(decimal), "0.01", "999999999999.99", ParseLimitsInInvariantCulture = true)] decimal Amount,
    DateOnly TransactionDate,
    [StringLength(1000)] string? Notes);

public sealed record QuickCategoryRequest(
    [Required, StringLength(80, MinimumLength = 2)] string Name,
    [StringLength(20)] string? Color);

public sealed record QuickFinancialCategoryRequest(
    [Required, StringLength(80, MinimumLength = 2)] string Name,
    TransactionType CategoryType,
    [StringLength(20)] string? Color);

public sealed record QuickAccountRequest(
    [Required, StringLength(100, MinimumLength = 2)] string Name,
    AccountType AccountType,
    [Range(typeof(decimal), "0", "999999999999.99", ParseLimitsInInvariantCulture = true)] decimal InitialBalance);
