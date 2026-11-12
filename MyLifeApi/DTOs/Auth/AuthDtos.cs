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

public sealed record RequestPasswordResetRequest(
    [Required, EmailAddress, StringLength(255)] string Email);

public sealed record ValidatePasswordResetCodeRequest(
    [Required, EmailAddress, StringLength(255)] string Email,
    [Required, RegularExpression("^[0-9]{6}$")] string Code);

public sealed record CompletePasswordResetRequest(
    [Required, EmailAddress, StringLength(255)] string Email,
    [Required, StringLength(128, MinimumLength = 32)] string ResetToken,
    [Required, StringLength(100, MinimumLength = 8)] string NewPassword);

public sealed record PasswordResetRequestedResponse(string Message, int CodeLifetimeMinutes);

public sealed record PasswordResetCodeValidatedResponse(string ResetToken);
