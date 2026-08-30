using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Transfers;

public sealed record TransferRequest(
    ulong UserId,
    ulong FromAccountId,
    ulong ToAccountId,
    [Range(typeof(decimal), "0.01", "999999999999.99", ParseLimitsInInvariantCulture = true)] decimal Amount,
    DateTime TransferDate,
    [StringLength(200)] string? Description,
    TransactionStatus Status = TransactionStatus.Completed);

public sealed record TransferResponse(
    ulong Id, ulong UserId, ulong FromAccountId, ulong ToAccountId,
    decimal Amount, DateTime TransferDate, string? Description,
    TransactionStatus Status, DateTime CreatedAt, DateTime UpdatedAt);
