using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Transactions;

public sealed record TransactionRequest(
    ulong UserId,
    ulong AccountId,
    ulong? CategoryId,
    ulong? RecurringTransactionId,
    TransactionType TransactionType,
    [Required, StringLength(200)] string Description,
    [Range(typeof(decimal), "0.01", "999999999999.99", ParseLimitsInInvariantCulture = true)] decimal Amount,
    DateOnly TransactionDate,
    TransactionStatus Status = TransactionStatus.Completed,
    string? Notes = null);

public sealed record TransactionResponse(
    ulong Id, ulong UserId, ulong AccountId, ulong? CategoryId,
    ulong? RecurringTransactionId, TransactionType TransactionType,
    string Description, decimal Amount, DateOnly TransactionDate,
    TransactionStatus Status, string? Notes,
    DateTime CreatedAt, DateTime UpdatedAt);
