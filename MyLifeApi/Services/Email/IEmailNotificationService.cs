using MyLifeApi.Models.Enums;

namespace MyLifeApi.Services.Email;

public sealed record FinancialMovementNotification(
    string UserName,
    string UserEmail,
    string AccountName,
    string Description,
    decimal Amount,
    DateOnly TransactionDate,
    TransactionType TransactionType);

public interface IEmailNotificationService
{
    Task NotifyFinancialMovementAsync(
        FinancialMovementNotification notification,
        CancellationToken cancellationToken = default);

    Task NotifyPasswordChangedAsync(
        string userName,
        string userEmail,
        DateTime changedAtUtc,
        CancellationToken cancellationToken = default);

    Task NotifyPasswordResetCodeAsync(
        string userName,
        string userEmail,
        string code,
        int lifetimeMinutes,
        CancellationToken cancellationToken = default);
}
