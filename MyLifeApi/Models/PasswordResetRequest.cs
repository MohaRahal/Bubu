namespace MyLifeApi.Models;

public sealed class PasswordResetRequest
{
    public ulong Id { get; set; }
    public ulong? UserId { get; set; }
    public string RequestedEmail { get; set; } = string.Empty;
    public string CodeHash { get; set; } = string.Empty;
    public string CodeSalt { get; set; } = string.Empty;
    public string? RequestedIp { get; set; }
    public string? UserAgent { get; set; }
    public uint FailedAttempts { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? ValidationTokenHash { get; set; }
    public DateTime? ValidatedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
