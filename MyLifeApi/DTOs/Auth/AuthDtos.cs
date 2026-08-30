using System.ComponentModel.DataAnnotations;

namespace MyLifeApi.DTOs.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(255)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

public sealed record AuthenticatedUserResponse(
    ulong Id,
    string Name,
    string Email,
    string? AvatarUrl);
