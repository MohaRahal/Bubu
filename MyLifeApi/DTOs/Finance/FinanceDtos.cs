using System.ComponentModel.DataAnnotations;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.DTOs.Finance;

public sealed record FinanceDashboardResponse(
    DateOnly From,
    DateOnly To,
    ulong? SelectedAccountId,
    FinanceSummaryResponse Summary,
    IReadOnlyList<FinanceAccountResponse> Accounts,
    IReadOnlyList<FinanceChartPointResponse> Chart,
    IReadOnlyList<FinanceMovementResponse> RecentMovements,
    IReadOnlyList<FinanceRecurringResponse> RecurringTransactions,
    IReadOnlyList<FinanceCategoryResponse> Categories);

public sealed record FinanceSummaryResponse(
    decimal Balance,
    decimal Income,
    decimal Expense,
    decimal PeriodResult,
    decimal IncomingTransfers,
    decimal OutgoingTransfers);

public sealed record FinanceAccountResponse(
    ulong Id,
    string Name,
    AccountType Type,
    decimal Balance,
    bool IsActive,
    bool HasMovements);

public sealed record FinanceChartPointResponse(
    DateOnly Date,
    decimal Income,
    decimal Expense,
    decimal Net,
    decimal Balance);

public sealed record FinanceMovementResponse(
    string Key,
    string Kind,
    string Description,
    decimal Amount,
    DateTime Date,
    DateTime CreatedAt,
    ulong? AccountId,
    string AccountName,
    string? DestinationAccountName,
    string? CategoryName,
    string? CategoryColor);

public sealed record FinanceCategoryResponse(
    ulong Id,
    string Name,
    TransactionType Type,
    string? Color);

public sealed record FinanceRecurringResponse(
    ulong Id,
    string Description,
    decimal Amount,
    RecurrenceFrequency Frequency,
    DateOnly NextDueDate,
    DateOnly? EndDate,
    bool IsActive,
    ulong AccountId,
    string AccountName,
    ulong? CategoryId,
    string? CategoryName,
    string? CategoryColor);

public sealed class CreateFinanceAccountRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }

    [Range(typeof(decimal), "0", "999999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal InitialBalance { get; set; }
}

public sealed class UpdateFinanceAccountRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public AccountType AccountType { get; set; }
}

public sealed class CreateFinanceTransactionRequest
{
    public TransactionType TransactionType { get; set; }
    public ulong AccountId { get; set; }
    public ulong? CategoryId { get; set; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; set; }

    public DateOnly TransactionDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public sealed class CreateFinanceCategoryRequest
{
    [Required, StringLength(80, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public TransactionType CategoryType { get; set; }

    [StringLength(20)]
    public string? Color { get; set; }
}

public sealed class CreateFinanceRecurringRequest
{
    public ulong AccountId { get; set; }
    public ulong? CategoryId { get; set; }

    [Required, StringLength(200, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; set; }

    public RecurrenceFrequency Frequency { get; set; }
    public DateOnly FirstDueDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public sealed class CreateFinanceTransferRequest
{
    public ulong FromAccountId { get; set; }
    public ulong ToAccountId { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; set; }

    public DateTime TransferDate { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }
}

public sealed class EmailFinanceReportRequest
{
    [Required]
    public IFormFile Report { get; set; } = null!;

    public DateOnly From { get; set; }

    public DateOnly To { get; set; }

    public ulong? AccountId { get; set; }
}
