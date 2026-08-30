using System.ComponentModel.DataAnnotations;

namespace MyLifeApi.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; init; } = string.Empty;

    [Required]
    public string Audience { get; init; } = string.Empty;

    [Required, MinLength(32)]
    public string Key { get; init; } = string.Empty;

    [Range(5, 1440)]
    public int AccessTokenMinutes { get; init; } = 30;

    [Required]
    public string CookieName { get; init; } = "bubu_access_token";
}
