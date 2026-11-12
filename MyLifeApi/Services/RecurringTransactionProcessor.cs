using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;
using MyLifeApi.Services.Email;

namespace MyLifeApi.Services;

public sealed class RecurringTransactionProcessor(
    MyLifeDbContext db,
    FinancialMovementEmailDispatcher emailDispatcher)
{
    private static readonly SemaphoreSlim ProcessingGate = new(1, 1);

    public Task ProcessAllDueAsync(CancellationToken cancellationToken) =>
        ProcessDueAsync(null, cancellationToken);

    public Task ProcessUserDueAsync(ulong userId, CancellationToken cancellationToken) =>
        ProcessDueAsync(userId, cancellationToken);

    private async Task ProcessDueAsync(ulong? userId, CancellationToken cancellationToken)
    {
        await ProcessingGate.WaitAsync(cancellationToken);
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var query = db.RecurringTransactions
                .Where(recurrence =>
                    recurrence.IsActive &&
                    recurrence.NextDueDate <= today &&
                    (!recurrence.EndDate.HasValue || recurrence.NextDueDate <= recurrence.EndDate));

            if (userId.HasValue)
                query = query.Where(recurrence => recurrence.UserId == userId.Value);

            var recurrences = await query.ToListAsync(cancellationToken);
            var generatedTransactions = new List<Transaction>();
            foreach (var recurrence in recurrences)
            {
                var generatedDateList = await db.Transactions
                    .AsNoTracking()
                    .Where(transaction => transaction.RecurringTransactionId == recurrence.Id)
                    .Select(transaction => transaction.TransactionDate)
                    .ToListAsync(cancellationToken);
                var generatedDates = generatedDateList.ToHashSet();

                while (recurrence.NextDueDate <= today &&
                       (!recurrence.EndDate.HasValue || recurrence.NextDueDate <= recurrence.EndDate.Value))
                {
                    var dueDate = recurrence.NextDueDate;
                    if (!generatedDates.Contains(dueDate))
                    {
                        var transaction = new Transaction
                        {
                            UserId = recurrence.UserId,
                            AccountId = recurrence.AccountId,
                            CategoryId = recurrence.CategoryId,
                            RecurringTransactionId = recurrence.Id,
                            TransactionType = recurrence.TransactionType,
                            Description = recurrence.Description,
                            Amount = recurrence.Amount,
                            TransactionDate = dueDate,
                            Status = TransactionStatus.Completed,
                            Notes = "Gerada automaticamente por uma recorrência."
                        };
                        db.Transactions.Add(transaction);
                        generatedTransactions.Add(transaction);
                        generatedDates.Add(dueDate);
                    }

                    recurrence.NextDueDate = GetNextDueDate(recurrence, dueDate);
                }

                if (recurrence.EndDate.HasValue && recurrence.NextDueDate > recurrence.EndDate.Value)
                    recurrence.IsActive = false;
            }

            if (recurrences.Count > 0)
            {
                await db.SaveChangesAsync(cancellationToken);
                foreach (var transaction in generatedTransactions)
                    await emailDispatcher.NotifyAsync(transaction, cancellationToken);
            }
        }
        finally
        {
            ProcessingGate.Release();
        }
    }

    public static DateOnly GetNextDueDate(RecurringTransaction recurrence, DateOnly current)
    {
        return recurrence.Frequency switch
        {
            RecurrenceFrequency.Daily => current.AddDays(1),
            RecurrenceFrequency.Weekly => current.AddDays(7),
            RecurrenceFrequency.Monthly => NextMonth(current, recurrence.StartDate.Day),
            RecurrenceFrequency.Yearly => NextYear(current, recurrence.StartDate.Month, recurrence.StartDate.Day),
            _ => throw new ArgumentOutOfRangeException(nameof(recurrence.Frequency))
        };
    }

    private static DateOnly NextMonth(DateOnly current, int anchorDay)
    {
        var nextMonth = current.AddMonths(1);
        return new DateOnly(
            nextMonth.Year,
            nextMonth.Month,
            Math.Min(anchorDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month)));
    }

    private static DateOnly NextYear(DateOnly current, int anchorMonth, int anchorDay)
    {
        var nextYear = current.Year + 1;
        return new DateOnly(
            nextYear,
            anchorMonth,
            Math.Min(anchorDay, DateTime.DaysInMonth(nextYear, anchorMonth)));
    }
}
