using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class FinancialCategory
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionType CategoryType { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}
