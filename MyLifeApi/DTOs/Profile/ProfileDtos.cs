using System.ComponentModel.DataAnnotations;

namespace MyLifeApi.DTOs.Profile;

public sealed record UpdateProfileRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(255)] string Email,
    [StringLength(255)] string? AvatarKey,
    DateOnly? BirthDate);

public sealed record ChangePasswordRequest(
    [Required, StringLength(100, MinimumLength = 8)] string CurrentPassword,
    [Required, StringLength(100, MinimumLength = 8)] string NewPassword);

public sealed record ProfileResponse(
    ulong Id,
    string Name,
    string Email,
    string? AvatarUrl,
    DateOnly? BirthDate,
    DateTime? LastLoginAt,
    DateTime CreatedAt);
