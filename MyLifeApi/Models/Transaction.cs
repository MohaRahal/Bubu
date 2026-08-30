using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class Transaction
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong AccountId { get; set; }
    public ulong? CategoryId { get; set; }
    public ulong? RecurringTransactionId { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly TransactionDate { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public FinancialCategory? Category { get; set; }
    public RecurringTransaction? RecurringTransaction { get; set; }
}
