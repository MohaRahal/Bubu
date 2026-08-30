using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class User
{
    public ulong Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public DateOnly? BirthDate { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime? LastLoginAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<Category> Categories { get; set; } = [];

    public ICollection<Activity> Activities { get; set; } = [];

    public ICollection<Goal> Goals { get; set; } = [];

    public ICollection<Account> Accounts { get; set; } = [];

    public ICollection<FinancialCategory> FinancialCategories { get; set; } = [];

    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = [];

    public ICollection<Transaction> Transactions { get; set; } = [];

    public ICollection<Transfer> Transfers { get; set; } = [];
}
