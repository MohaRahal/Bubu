using MyLifeApi.Models.Enums;

namespace MyLifeApi.Models;

public sealed class GoalContribution
{
    public ulong Id { get; set; }
    public ulong GoalId { get; set; }
    public ulong? AccountId { get; set; }
    public GoalContributionType ContributionType { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public Goal Goal { get; set; } = null!;
    public Account? Account { get; set; }
}
