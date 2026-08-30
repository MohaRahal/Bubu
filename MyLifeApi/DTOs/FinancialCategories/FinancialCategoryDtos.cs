using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.FinancialCategories;

public sealed record FinancialCategoryRequest(
    ulong UserId,
    [Required, StringLength(80)] string Name,
    TransactionType CategoryType,
    [StringLength(50)] string? Icon,
    [StringLength(20)] string? Color,
    bool IsActive = true);

public sealed record FinancialCategoryResponse(
    ulong Id, ulong UserId, string Name, TransactionType CategoryType,
    string? Icon, string? Color, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);
