using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Services;

public sealed class AccountBalanceService(MyLifeDbContext db)
{
    public async Task<IReadOnlyDictionary<ulong, decimal>> CalculateAsync(
        ulong userId,
        CancellationToken cancellationToken = default)
    {
        var balances = await db.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId)
            .ToDictionaryAsync(
                account => account.Id,
                account => account.InitialBalance,
                cancellationToken);

        var transactionTotals = await db.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Status == TransactionStatus.Completed)
            .GroupBy(transaction => transaction.AccountId)
            .Select(group => new
            {
                AccountId = group.Key,
                Amount = group.Sum(transaction => transaction.TransactionType == TransactionType.Income
                    ? transaction.Amount
                    : -transaction.Amount)
            })
            .ToListAsync(cancellationToken);

        var transferTotals = await db.Transfers
            .AsNoTracking()
            .Where(transfer => transfer.UserId == userId && transfer.Status == TransactionStatus.Completed)
            .Select(transfer => new
            {
                transfer.FromAccountId,
                transfer.ToAccountId,
                transfer.Amount
            })
            .ToListAsync(cancellationToken);

        var goalTotals = await db.GoalContributions
            .AsNoTracking()
            .Where(contribution =>
                contribution.Goal.UserId == userId &&
                contribution.AccountId.HasValue)
            .GroupBy(contribution => contribution.AccountId!.Value)
            .Select(group => new
            {
                AccountId = group.Key,
                Amount = group.Sum(contribution =>
                    contribution.ContributionType == GoalContributionType.Withdrawal
                        ? contribution.Amount
                        : -contribution.Amount)
            })
            .ToListAsync(cancellationToken);

        foreach (var total in transactionTotals)
            if (balances.ContainsKey(total.AccountId)) balances[total.AccountId] += total.Amount;

        foreach (var transfer in transferTotals)
        {
            if (balances.ContainsKey(transfer.FromAccountId)) balances[transfer.FromAccountId] -= transfer.Amount;
            if (balances.ContainsKey(transfer.ToAccountId)) balances[transfer.ToAccountId] += transfer.Amount;
        }

        foreach (var total in goalTotals)
            if (balances.ContainsKey(total.AccountId)) balances[total.AccountId] += total.Amount;

        return balances;
    }
}
