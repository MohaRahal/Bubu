using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class RecurringTransaction
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong AccountId { get; set; }
    public ulong? CategoryId { get; set; }
    public TransactionType TransactionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public RecurrenceFrequency Frequency { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly NextDueDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Account Account { get; set; } = null!;
    public FinancialCategory? Category { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = [];
}
