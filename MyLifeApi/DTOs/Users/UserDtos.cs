using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Users;

public sealed record CreateUserRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(255)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password,
    [Url, StringLength(500)] string? AvatarUrl,
    DateOnly? BirthDate);

public sealed record UpdateUserRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Name,
    [Required, EmailAddress, StringLength(255)] string Email,
    [Url, StringLength(500)] string? AvatarUrl,
    DateOnly? BirthDate,
    UserStatus Status);

public sealed record UserResponse(
    ulong Id,
    string Name,
    string Email,
    string? AvatarUrl,
    DateOnly? BirthDate,
    UserStatus Status,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);
