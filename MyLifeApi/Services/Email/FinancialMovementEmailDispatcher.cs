using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Services.Email;

public sealed class FinancialMovementEmailDispatcher(
    MyLifeDbContext db,
    IEmailNotificationService notifications,
    ILogger<FinancialMovementEmailDispatcher> logger)
{
    public async Task NotifyAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        if (transaction.Status != TransactionStatus.Completed)
            return;

        try
        {
            await NotifyCoreAsync(transaction, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Não foi possível preparar a notificação da movimentação {TransactionId}.",
                transaction.Id);
        }
    }

    private async Task NotifyCoreAsync(
        Transaction transaction,
        CancellationToken cancellationToken)
    {
        var details = await db.Accounts
            .AsNoTracking()
            .Where(account =>
                account.Id == transaction.AccountId &&
                account.UserId == transaction.UserId)
            .Select(account => new
            {
                AccountName = account.Name,
                UserName = account.User.Name,
                UserEmail = account.User.Email
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (details is null)
        {
            logger.LogWarning(
                "A notificação da movimentação {TransactionId} não foi criada porque usuário ou conta não foram encontrados.",
                transaction.Id);
            return;
        }

        await notifications.NotifyFinancialMovementAsync(
            new FinancialMovementNotification(
                details.UserName,
                details.UserEmail,
                details.AccountName,
                transaction.Description,
                transaction.Amount,
                transaction.TransactionDate,
                transaction.TransactionType),
            cancellationToken);
    }
}
