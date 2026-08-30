using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs.Dashboard;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!ulong.TryParse(subject, out var userId))
            return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Now);
        var weekStart = today.AddDays(-6);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var weekActivities = await db.Activities
            .AsNoTracking()
            .Where(activity =>
                activity.UserId == userId &&
                activity.ActivityDate >= weekStart &&
                activity.ActivityDate <= today &&
                activity.Status != ActivityStatus.Cancelled)
            .Select(activity => new
            {
                activity.ActivityDate,
                activity.Status,
                activity.DurationSeconds
            })
            .ToListAsync(cancellationToken);

        var monthTransactions = await db.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.TransactionDate >= monthStart &&
                transaction.TransactionDate <= today &&
                transaction.Status == TransactionStatus.Completed)
            .Select(transaction => new
            {
                transaction.TransactionDate,
                transaction.TransactionType,
                transaction.Amount
            })
            .ToListAsync(cancellationToken);

        var initialBalance = await db.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId && account.IsActive)
            .SumAsync(account => (decimal?)account.InitialBalance, cancellationToken) ?? 0;

        var transactionBalance = await db.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Account.IsActive &&
                transaction.Status == TransactionStatus.Completed)
            .SumAsync(
                transaction => (decimal?)(transaction.TransactionType == TransactionType.Income
                    ? transaction.Amount
                    : -transaction.Amount),
                cancellationToken) ?? 0;

        var upcomingActivities = await db.Activities
            .AsNoTracking()
            .Where(activity =>
                activity.UserId == userId &&
                activity.ActivityDate >= today &&
                (activity.Status == ActivityStatus.Planned ||
                 activity.Status == ActivityStatus.InProgress))
            .OrderBy(activity => activity.ActivityDate)
            .ThenBy(activity => activity.StartedAt)
            .Take(5)
            .Select(activity => new DashboardActivityResponse(
                activity.Id,
                activity.Title,
                activity.ActivityDate,
                activity.StartedAt,
                activity.Status,
                activity.Category != null ? activity.Category.Name : null,
                activity.Category != null ? activity.Category.Color : null))
            .ToListAsync(cancellationToken);

        var goalData = await db.Goals
            .AsNoTracking()
            .Where(goal => goal.UserId == userId && goal.Status == GoalStatus.Active)
            .OrderBy(goal => goal.TargetDate)
            .Take(4)
            .Select(goal => new
            {
                goal.Id,
                goal.Title,
                goal.TargetDate,
                goal.TargetValue,
                goal.Unit,
                TotalActivities = goal.ActivityGoals.Count,
                CompletedActivities = goal.ActivityGoals.Count(link =>
                    link.Activity.Status == ActivityStatus.Completed)
            })
            .ToListAsync(cancellationToken);

        var activeGoals = goalData
            .Select(goal => new DashboardGoalResponse(
                goal.Id,
                goal.Title,
                goal.TargetDate,
                goal.TargetValue,
                goal.Unit,
                goal.CompletedActivities,
                goal.TotalActivities,
                goal.TotalActivities == 0
                    ? 0
                    : decimal.Round(
                        goal.CompletedActivities * 100m / goal.TotalActivities,
                        1)))
            .ToList();

        var recentTransactions = await db.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Status != TransactionStatus.Cancelled)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Take(5)
            .Select(transaction => new DashboardTransactionResponse(
                transaction.Id,
                transaction.Description,
                transaction.Amount,
                transaction.TransactionDate,
                transaction.TransactionType,
                transaction.Account.Name,
                transaction.Category != null ? transaction.Category.Name : null))
            .ToListAsync(cancellationToken);

        var todayActivities = weekActivities
            .Where(activity => activity.ActivityDate == today)
            .ToList();
        var completedToday = todayActivities.Count(activity =>
            activity.Status == ActivityStatus.Completed);
        var focusSecondsToday = todayActivities.Sum(activity =>
            (long)(activity.DurationSeconds ?? 0));
        var monthIncome = monthTransactions
            .Where(transaction => transaction.TransactionType == TransactionType.Income)
            .Sum(transaction => transaction.Amount);
        var monthExpense = monthTransactions
            .Where(transaction => transaction.TransactionType == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);

        var chart = Enumerable.Range(0, 7)
            .Select(offset => weekStart.AddDays(offset))
            .Select(date => new DashboardChartPointResponse(
                date,
                weekActivities.Count(activity =>
                    activity.ActivityDate == date &&
                    activity.Status == ActivityStatus.Completed),
                weekActivities
                    .Where(activity => activity.ActivityDate == date)
                    .Sum(activity => (long)(activity.DurationSeconds ?? 0)),
                monthTransactions
                    .Where(transaction =>
                        transaction.TransactionDate == date &&
                        transaction.TransactionType == TransactionType.Income)
                    .Sum(transaction => transaction.Amount),
                monthTransactions
                    .Where(transaction =>
                        transaction.TransactionDate == date &&
                        transaction.TransactionType == TransactionType.Expense)
                    .Sum(transaction => transaction.Amount)))
            .ToList();

        var summary = new DashboardSummaryResponse(
            completedToday,
            todayActivities.Count,
            focusSecondsToday,
            goalData.Count,
            initialBalance + transactionBalance,
            monthIncome,
            monthExpense);

        return Ok(new DashboardResponse(
            summary,
            chart,
            upcomingActivities,
            activeGoals,
            recentTransactions,
            CreateInsight(summary)));
    }

    private static string CreateInsight(DashboardSummaryResponse summary)
    {
        if (summary.TotalActivitiesToday == 0 && summary.ActiveGoals == 0)
            return "Comece com algo pequeno: registre uma atividade ou crie seu primeiro objetivo.";

        if (summary.TotalActivitiesToday > 0 &&
            summary.CompletedActivitiesToday * 100 / summary.TotalActivitiesToday >= 70)
            return "Seu dia está avançando bem. Você já concluiu a maior parte do que planejou.";

        if (summary.MonthExpense > summary.MonthIncome && summary.MonthExpense > 0)
            return "Suas despesas do mês estão acima das receitas. Vale revisar os últimos lançamentos.";

        if (summary.FocusSecondsToday >= 3600)
            return "Você já acumulou mais de uma hora de foco hoje. Bom ritmo — lembre de fazer uma pausa.";

        return "Seu espaço está tomando forma. Continue registrando para o Bubu encontrar padrões úteis.";
    }
}
