using System.IdentityModel.Tokens.Jwt;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs.Goals;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;
using MyLifeApi.Services;

namespace MyLifeApi.Controllers;

[ApiController]
[Authorize]
[Route("api/goals")]
public sealed class GoalsController(
    MyLifeDbContext db,
    AccountBalanceService accountBalanceService) : ControllerBase
{
    [HttpGet("account-options")]
    public async Task<ActionResult<IReadOnlyList<GoalAccountOptionResponse>>> GetAccountOptions(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var balances = await accountBalanceService.CalculateAsync(userId, cancellationToken);
        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId && account.IsActive)
            .OrderBy(account => account.Name)
            .ToListAsync(cancellationToken);

        return Ok(accounts.Select(account => new GoalAccountOptionResponse(
            account.Id,
            account.Name,
            balances.GetValueOrDefault(account.Id))).ToList());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GoalVaultResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var goals = await db.Goals
            .AsNoTracking()
            .Include(goal => goal.Contributions)
                .ThenInclude(contribution => contribution.Account)
            .Where(goal => goal.UserId == userId && goal.Status != GoalStatus.Cancelled)
            .OrderBy(goal => goal.Status == GoalStatus.Completed)
            .ThenBy(goal => goal.TargetDate)
            .ThenByDescending(goal => goal.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(goals.Select(ToVaultResponse).ToList());
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<GoalVaultResponse>> GetById(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var goal = await db.Goals
            .AsNoTracking()
            .Include(item => item.Contributions)
                .ThenInclude(contribution => contribution.Account)
            .SingleOrDefaultAsync(
                item => item.Id == id && item.UserId == userId,
                cancellationToken);

        return goal is null ? NotFound() : Ok(ToVaultResponse(goal));
    }

    [HttpPost]
    public async Task<ActionResult<GoalVaultResponse>> Create(
        GoalVaultRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (request.TargetDate.HasValue && request.TargetDate < today)
            return BadRequest(new ProblemDetails { Title = "A data da meta não pode estar no passado." });

        var goal = new Goal
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            GoalType = GoalType.Money,
            TargetValue = request.TargetValue,
            Unit = "R$",
            StartDate = today,
            TargetDate = request.TargetDate,
            Status = GoalStatus.Active
        };

        db.Goals.Add(goal);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, ToVaultResponse(goal));
    }

    [HttpPost("{id:long}/contributions")]
    public async Task<ActionResult<GoalVaultResponse>> AddContribution(
        ulong id,
        GoalContributionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        await using var databaseTransaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var goal = await db.Goals
            .Include(item => item.Contributions)
                .ThenInclude(contribution => contribution.Account)
            .SingleOrDefaultAsync(
                item => item.Id == id && item.UserId == userId,
                cancellationToken);

        if (goal is null) return NotFound();
        if (goal.Status == GoalStatus.Cancelled)
            return BadRequest(new ProblemDetails { Title = "Esse objetivo foi cancelado." });

        Account? account = null;
        if (request.AccountId.HasValue)
        {
            account = await db.Accounts.SingleOrDefaultAsync(
                item => item.Id == request.AccountId.Value &&
                        item.UserId == userId &&
                        item.IsActive,
                cancellationToken);
            if (account is null)
                return BadRequest(new ProblemDetails { Title = "Selecione uma conta válida." });
        }

        var currentValue = CalculateCurrentValue(goal.Contributions);
        if (request.ContributionType == GoalContributionType.Withdrawal && request.Amount > currentValue)
            return BadRequest(new ProblemDetails
            {
                Title = "O valor retirado não pode ser maior que o saldo do cofre."
            });

        if (account is not null && request.ContributionType == GoalContributionType.Deposit)
        {
            var balances = await accountBalanceService.CalculateAsync(userId, cancellationToken);
            if (request.Amount > balances.GetValueOrDefault(account.Id))
                return BadRequest(new ProblemDetails
                {
                    Title = "Saldo insuficiente na conta selecionada."
                });
        }

        goal.Contributions.Add(new GoalContribution
        {
            AccountId = account?.Id,
            Account = account,
            ContributionType = request.ContributionType,
            Amount = request.Amount,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim()
        });

        var updatedValue = request.ContributionType == GoalContributionType.Deposit
            ? currentValue + request.Amount
            : currentValue - request.Amount;

        if (goal.TargetValue.HasValue)
        {
            if (updatedValue >= goal.TargetValue.Value)
                goal.Status = GoalStatus.Completed;
            else if (goal.Status == GoalStatus.Completed)
                goal.Status = GoalStatus.Active;
        }

        await db.SaveChangesAsync(cancellationToken);
        await databaseTransaction.CommitAsync(cancellationToken);
        return Ok(ToVaultResponse(goal));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var goal = await db.Goals.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == userId,
            cancellationToken);

        if (goal is null) return NotFound();

        db.Goals.Remove(goal);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private bool TryGetUserId(out ulong userId)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return ulong.TryParse(subject, out userId);
    }

    private static decimal CalculateCurrentValue(IEnumerable<GoalContribution> contributions) =>
        contributions.Sum(contribution =>
            contribution.ContributionType == GoalContributionType.Deposit
                ? contribution.Amount
                : -contribution.Amount);

    private static GoalVaultResponse ToVaultResponse(Goal goal)
    {
        var currentValue = CalculateCurrentValue(goal.Contributions);
        var targetValue = goal.TargetValue ?? 0;
        var progress = targetValue <= 0
            ? 0
            : Math.Min(100, Math.Round(currentValue / targetValue * 100, 1));
        var recentContributions = goal.Contributions
            .OrderByDescending(contribution => contribution.CreatedAt)
            .ThenByDescending(contribution => contribution.Id)
            .Take(5)
            .Select(contribution => new GoalContributionResponse(
                contribution.Id,
                contribution.ContributionType,
                contribution.AccountId,
                contribution.Account?.Name,
                contribution.Amount,
                contribution.Note,
                contribution.CreatedAt))
            .ToList();

        return new GoalVaultResponse(
            goal.Id,
            goal.Title,
            goal.Description,
            goal.GoalType,
            goal.Unit,
            targetValue,
            currentValue,
            progress,
            goal.TargetDate,
            goal.Status,
            recentContributions,
            goal.CreatedAt);
    }
}
