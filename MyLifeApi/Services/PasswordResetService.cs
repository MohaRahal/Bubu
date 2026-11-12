using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyLifeApi.Data;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;
using MyLifeApi.Options;
using MyLifeApi.Services.Email;

namespace MyLifeApi.Services;

public sealed class PasswordResetService(
    MyLifeDbContext db,
    PasswordResetCodeService codeService,
    IPasswordHasher<User> passwordHasher,
    IEmailNotificationService emailNotifications,
    IOptions<PasswordResetOptions> options)
{
    private static readonly TimeSpan MinimumRequestDuration = TimeSpan.FromMilliseconds(750);
    private readonly PasswordResetOptions _options = options.Value;

    public int CodeLifetimeMinutes => _options.CodeLifetimeMinutes;

    public async Task RequestAsync(
        string emailAddress,
        string? requesterIp,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        var startedAt = Stopwatch.GetTimestamp();
        var email = NormalizeEmail(emailAddress);
        var now = DateTime.UtcNow;
        var user = await db.Users.SingleOrDefaultAsync(
            candidate => candidate.Email == email && candidate.Status == UserStatus.Active,
            cancellationToken);

        if (user is not null)
        {
            var latestRequest = await db.PasswordResetRequests
                .AsNoTracking()
                .Where(request => request.UserId == user.Id)
                .OrderByDescending(request => request.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
            var cooldown = TimeSpan.FromSeconds(_options.RequestCooldownSeconds);

            if (latestRequest is null || now - latestRequest.CreatedAt >= cooldown)
            {
                var activeRequests = await db.PasswordResetRequests
                    .Where(request =>
                        request.UserId == user.Id &&
                        request.UsedAt == null &&
                        request.RevokedAt == null)
                    .ToListAsync(cancellationToken);

                foreach (var activeRequest in activeRequests)
                    activeRequest.RevokedAt = now;

                var code = PasswordResetCodeService.GenerateCode();
                var salt = PasswordResetCodeService.GenerateSalt();
                var resetRequest = new PasswordResetRequest
                {
                    UserId = user.Id,
                    RequestedEmail = email,
                    CodeSalt = salt,
                    CodeHash = codeService.Hash(salt, code),
                    RequestedIp = Truncate(requesterIp, 45),
                    UserAgent = Truncate(userAgent, 512),
                    FailedAttempts = 0,
                    ExpiresAt = now.AddMinutes(_options.CodeLifetimeMinutes),
                    CreatedAt = now
                };

                db.PasswordResetRequests.Add(resetRequest);
                await db.SaveChangesAsync(cancellationToken);
                await emailNotifications.NotifyPasswordResetCodeAsync(
                    user.Name,
                    user.Email,
                    code,
                    _options.CodeLifetimeMinutes,
                    cancellationToken);
            }
        }

        var elapsed = Stopwatch.GetElapsedTime(startedAt);
        if (elapsed < MinimumRequestDuration)
            await Task.Delay(MinimumRequestDuration - elapsed, cancellationToken);
    }

    public async Task<string?> ValidateCodeAsync(
        string emailAddress,
        string code,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(emailAddress);
        var now = DateTime.UtcNow;
        var resetRequest = await db.PasswordResetRequests
            .Include(request => request.User)
            .Where(request =>
                request.RequestedEmail == email &&
                request.UsedAt == null &&
                request.RevokedAt == null)
            .OrderByDescending(request => request.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetRequest?.User is null || resetRequest.User.Status != UserStatus.Active)
            return null;

        if (resetRequest.ExpiresAt <= now || resetRequest.FailedAttempts >= (uint)_options.MaxAttempts)
        {
            resetRequest.RevokedAt = now;
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (!codeService.Verify(resetRequest.CodeSalt, resetRequest.CodeHash, code))
        {
            resetRequest.FailedAttempts++;
            if (resetRequest.FailedAttempts >= (uint)_options.MaxAttempts)
                resetRequest.RevokedAt = now;

            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        var validationToken = PasswordResetCodeService.GenerateValidationToken();
        resetRequest.ValidationTokenHash = codeService.Hash(resetRequest.CodeSalt, validationToken);
        resetRequest.ValidatedAt = now;
        await db.SaveChangesAsync(cancellationToken);
        return validationToken;
    }

    public async Task<bool> CompleteAsync(
        string emailAddress,
        string validationToken,
        string newPassword,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(emailAddress);
        var now = DateTime.UtcNow;
        var resetRequest = await db.PasswordResetRequests
            .Include(request => request.User)
            .Where(request =>
                request.RequestedEmail == email &&
                request.ValidationTokenHash != null &&
                request.ValidatedAt != null &&
                request.UsedAt == null &&
                request.RevokedAt == null)
            .OrderByDescending(request => request.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetRequest?.User is null ||
            resetRequest.User.Status != UserStatus.Active ||
            resetRequest.ExpiresAt <= now ||
            !codeService.Verify(
                resetRequest.CodeSalt,
                resetRequest.ValidationTokenHash!,
                validationToken))
            return false;

        resetRequest.User.PasswordHash = passwordHasher.HashPassword(
            resetRequest.User,
            newPassword);
        resetRequest.UsedAt = now;

        var otherActiveRequests = await db.PasswordResetRequests
            .Where(request =>
                request.UserId == resetRequest.UserId &&
                request.Id != resetRequest.Id &&
                request.UsedAt == null &&
                request.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var activeRequest in otherActiveRequests)
            activeRequest.RevokedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        await emailNotifications.NotifyPasswordChangedAsync(
            resetRequest.User.Name,
            resetRequest.User.Email,
            now,
            cancellationToken);
        return true;
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static string? Truncate(string? value, int maxLength) =>
        string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim()[..Math.Min(value.Trim().Length, maxLength)];
}
