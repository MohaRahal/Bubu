using System.ComponentModel.DataAnnotations;

namespace MyLifeApi.Options;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    [Range(5, 30)]
    public int CodeLifetimeMinutes { get; set; } = 10;

    [Range(30, 300)]
    public int RequestCooldownSeconds { get; set; } = 60;

    [Range(3, 10)]
    public int MaxAttempts { get; set; } = 5;

    [MinLength(32)]
    public string HashKey { get; set; } = string.Empty;
}
