using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs.QuickRegister;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;
using MyLifeApi.Services.Email;

namespace MyLifeApi.Controllers;

[ApiController]
[Authorize]
[Route("api/quick-register")]
public sealed class QuickRegisterController(
    MyLifeDbContext db,
    FinancialMovementEmailDispatcher emailDispatcher) : ControllerBase
{
    [HttpGet("options")]
    public async Task<ActionResult<QuickRegisterOptionsResponse>> GetOptions(
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var accounts = await db.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId && account.IsActive)
            .OrderBy(account => account.Name)
            .Select(account => new QuickAccountResponse(account.Id, account.Name))
            .ToListAsync(cancellationToken);

        var financialCategories = await db.FinancialCategories
            .AsNoTracking()
            .Where(category => category.UserId == userId && category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new QuickFinancialCategoryResponse(
                category.Id,
                category.Name,
                category.CategoryType,
                category.Color))
            .ToListAsync(cancellationToken);

        var activityCategories = await db.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId && category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new QuickActivityCategoryResponse(
                category.Id,
                category.Name,
                category.Color))
            .ToListAsync(cancellationToken);

        return Ok(new QuickRegisterOptionsResponse(
            accounts,
            financialCategories,
            activityCategories));
    }

    [HttpPost("goals")]
    public async Task<IActionResult> CreateGoal(
        QuickGoalRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        if (request.TargetDate < request.StartDate)
            return BadRequest(new ProblemDetails { Title = "A data final não pode ser anterior à data inicial." });

        if (request.GoalType != GoalType.Boolean && !request.TargetValue.HasValue)
            return BadRequest(new ProblemDetails { Title = "Informe o valor que deseja alcançar." });

        var goal = new Goal
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            GoalType = request.GoalType,
            TargetValue = request.GoalType == GoalType.Boolean ? 1 : request.TargetValue,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? null : request.Unit.Trim(),
            StartDate = request.StartDate,
            TargetDate = request.TargetDate,
            Status = GoalStatus.Active
        };

        db.Goals.Add(goal);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/goals/{goal.Id}", new { goal.Id });
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction(
        QuickTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var accountExists = await db.Accounts.AnyAsync(
            account => account.Id == request.AccountId &&
                       account.UserId == userId &&
                       account.IsActive,
            cancellationToken);

        if (!accountExists)
            return BadRequest(new ProblemDetails { Title = "Selecione uma conta válida." });

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await db.FinancialCategories.AnyAsync(
                category => category.Id == request.CategoryId &&
                            category.UserId == userId &&
                            category.IsActive &&
                            category.CategoryType == request.TransactionType,
                cancellationToken);

            if (!categoryExists)
                return BadRequest(new ProblemDetails { Title = "Selecione uma categoria compatível." });
        }

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            TransactionType = request.TransactionType,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            TransactionDate = request.TransactionDate,
            Status = TransactionStatus.Completed,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        await emailDispatcher.NotifyAsync(transaction, cancellationToken);
        return Created($"/api/transactions/{transaction.Id}", new { transaction.Id });
    }

    [HttpPost("categories")]
    public async Task<ActionResult<QuickActivityCategoryResponse>> CreateCategory(
        QuickCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var name = request.Name.Trim();

        if (await db.Categories.AnyAsync(
                category => category.UserId == userId && category.Name == name,
                cancellationToken))
            return Conflict(new ProblemDetails { Title = "Você já possui uma categoria com esse nome." });

        var category = new Category
        {
            UserId = userId,
            Name = name,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#70c52f" : request.Color,
            IsActive = true
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new QuickActivityCategoryResponse(category.Id, category.Name, category.Color));
    }

    [HttpPost("financial-categories")]
    public async Task<ActionResult<QuickFinancialCategoryResponse>> CreateFinancialCategory(
        QuickFinancialCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var name = request.Name.Trim();

        if (await db.FinancialCategories.AnyAsync(
                category => category.UserId == userId &&
                            category.Name == name &&
                            category.CategoryType == request.CategoryType,
                cancellationToken))
            return Conflict(new ProblemDetails { Title = "Essa categoria financeira já existe." });

        var category = new FinancialCategory
        {
            UserId = userId,
            Name = name,
            CategoryType = request.CategoryType,
            Color = string.IsNullOrWhiteSpace(request.Color) ? "#70c52f" : request.Color,
            IsActive = true
        };
        db.FinancialCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new QuickFinancialCategoryResponse(
            category.Id,
            category.Name,
            category.CategoryType,
            category.Color));
    }

    [HttpPost("accounts")]
    public async Task<ActionResult<QuickAccountResponse>> CreateAccount(
        QuickAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var name = request.Name.Trim();

        if (await db.Accounts.AnyAsync(
                account => account.UserId == userId && account.Name == name && account.IsActive,
                cancellationToken))
            return Conflict(new ProblemDetails { Title = "Você já possui uma conta com esse nome." });

        var account = new Account
        {
            UserId = userId,
            Name = name,
            AccountType = request.AccountType,
            InitialBalance = request.InitialBalance,
            IsActive = true
        };
        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        return Ok(new QuickAccountResponse(account.Id, account.Name));
    }

    private bool TryGetUserId(out ulong userId)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return ulong.TryParse(subject, out userId);
    }
}
