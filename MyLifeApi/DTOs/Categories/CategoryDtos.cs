using System.ComponentModel.DataAnnotations;

namespace MyLifeApi.DTOs.Categories;

public sealed record CategoryRequest(
    ulong UserId,
    [Required, StringLength(80)] string Name,
    [StringLength(255)] string? Description,
    [StringLength(50)] string? Icon,
    [StringLength(20)] string? Color,
    bool IsActive = true);

public sealed record CategoryResponse(
    ulong Id, ulong UserId, string Name, string? Description,
    string? Icon, string? Color, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);
