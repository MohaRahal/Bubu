using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.RecurringTransactions;

public sealed record RecurringTransactionRequest(
    ulong UserId,
    ulong AccountId,
    ulong? CategoryId,
    TransactionType TransactionType,
    [Required, StringLength(200)] string Description,
    [Range(typeof(decimal), "0.01", "999999999999.99")] decimal Amount,
    RecurrenceFrequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    DateOnly NextDueDate,
    bool IsActive = true);

public sealed record RecurringTransactionResponse(
    ulong Id, ulong UserId, ulong AccountId, ulong? CategoryId,
    TransactionType TransactionType, string Description, decimal Amount,
    RecurrenceFrequency Frequency, DateOnly StartDate, DateOnly? EndDate,
    DateOnly NextDueDate, bool IsActive,
    DateTime CreatedAt, DateTime UpdatedAt);
