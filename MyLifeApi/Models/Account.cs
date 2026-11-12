using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class Account
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public decimal InitialBalance { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
    public ICollection<Transfer> OutgoingTransfers { get; set; } = [];
    public ICollection<Transfer> IncomingTransfers { get; set; } = [];
    public ICollection<GoalContribution> GoalContributions { get; set; } = [];
}
