using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public class Transfer
{
    public ulong Id { get; set; }
    public ulong UserId { get; set; }
    public ulong FromAccountId { get; set; }
    public ulong ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string? Description { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public Account FromAccount { get; set; } = null!;
    public Account ToAccount { get; set; } = null!;
}
