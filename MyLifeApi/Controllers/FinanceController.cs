using System.IdentityModel.Tokens.Jwt;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs.Finance;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;
using MyLifeApi.Services;

namespace MyLifeApi.Controllers;

[ApiController]
[Authorize]
[Route("api/finance")]
public sealed class FinanceController(
    MyLifeDbContext db,
    RecurringTransactionProcessor recurringProcessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FinanceDashboardResponse>> GetDashboard(
        [FromQuery] ulong? accountId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var today = DateOnly.FromDateTime(DateTime.Today);
        await recurringProcessor.ProcessUserDueAsync(userId, cancellationToken);
        var startDate = from ?? new DateOnly(today.Year, today.Month, 1);
        var endDate = to ?? today;

        if (endDate < startDate)
            return BadRequest(new ProblemDetails { Title = "A data final não pode ser anterior à inicial." });

        if (endDate.DayNumber - startDate.DayNumber > 366)
            return BadRequest(new ProblemDetails { Title = "Escolha um período de até 12 meses." });

        var accountEntities = await db.Accounts
            .AsNoTracking()
            .Where(account => account.UserId == userId && account.IsActive)
            .OrderBy(account => account.Name)
            .ToListAsync(cancellationToken);

        if (accountId.HasValue && accountEntities.All(account => account.Id != accountId.Value))
            return NotFound(new ProblemDetails { Title = "A conta selecionada não foi encontrada." });

        var transactionTotals = await db.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.UserId == userId && transaction.Status == TransactionStatus.Completed)
            .GroupBy(transaction => transaction.AccountId)
            .Select(group => new
            {
                AccountId = group.Key,
                Balance = group.Sum(transaction => transaction.TransactionType == TransactionType.Income
                    ? transaction.Amount
                    : -transaction.Amount)
            })
            .ToDictionaryAsync(item => item.AccountId, item => item.Balance, cancellationToken);

        var incomingTotals = await db.Transfers
            .AsNoTracking()
            .Where(transfer => transfer.UserId == userId && transfer.Status == TransactionStatus.Completed)
            .GroupBy(transfer => transfer.ToAccountId)
            .Select(group => new { AccountId = group.Key, Amount = group.Sum(transfer => transfer.Amount) })
            .ToDictionaryAsync(item => item.AccountId, item => item.Amount, cancellationToken);

        var outgoingTotals = await db.Transfers
            .AsNoTracking()
            .Where(transfer => transfer.UserId == userId && transfer.Status == TransactionStatus.Completed)
            .GroupBy(transfer => transfer.FromAccountId)
            .Select(group => new { AccountId = group.Key, Amount = group.Sum(transfer => transfer.Amount) })
            .ToDictionaryAsync(item => item.AccountId, item => item.Amount, cancellationToken);

        decimal AccountBalance(Account account) =>
            account.InitialBalance +
            transactionTotals.GetValueOrDefault(account.Id) +
            incomingTotals.GetValueOrDefault(account.Id) -
            outgoingTotals.GetValueOrDefault(account.Id);

        var accountsWithMovements = (await db.Transactions
                .AsNoTracking()
                .Where(transaction => transaction.UserId == userId)
                .Select(transaction => transaction.AccountId)
                .Distinct()
                .ToListAsync(cancellationToken))
            .ToHashSet();
        accountsWithMovements.UnionWith(await db.Transfers
            .AsNoTracking()
            .Where(transfer => transfer.UserId == userId)
            .Select(transfer => transfer.FromAccountId)
            .Distinct()
            .ToListAsync(cancellationToken));
        accountsWithMovements.UnionWith(await db.Transfers
            .AsNoTracking()
            .Where(transfer => transfer.UserId == userId)
            .Select(transfer => transfer.ToAccountId)
            .Distinct()
            .ToListAsync(cancellationToken));
        accountsWithMovements.UnionWith(await db.RecurringTransactions
            .AsNoTracking()
            .Where(recurrence => recurrence.UserId == userId)
            .Select(recurrence => recurrence.AccountId)
            .Distinct()
            .ToListAsync(cancellationToken));

        var accounts = accountEntities
            .Select(account => new FinanceAccountResponse(
                account.Id,
                account.Name,
                account.AccountType,
                AccountBalance(account),
                account.IsActive,
                accountsWithMovements.Contains(account.Id)))
            .ToList();

        var periodTransactionsQuery = db.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Status == TransactionStatus.Completed &&
                transaction.TransactionDate >= startDate &&
                transaction.TransactionDate <= endDate);

        if (accountId.HasValue)
            periodTransactionsQuery = periodTransactionsQuery.Where(transaction => transaction.AccountId == accountId.Value);

        var periodTransactions = await periodTransactionsQuery
            .Select(transaction => new
            {
                transaction.Id,
                transaction.AccountId,
                AccountName = transaction.Account.Name,
                transaction.TransactionType,
                transaction.Description,
                transaction.Amount,
                transaction.TransactionDate,
                CategoryName = transaction.Category == null ? null : transaction.Category.Name,
                CategoryColor = transaction.Category == null ? null : transaction.Category.Color,
                transaction.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var periodStart = startDate.ToDateTime(TimeOnly.MinValue);
        var periodEnd = endDate.ToDateTime(TimeOnly.MaxValue);
        var periodTransfersQuery = db.Transfers
            .AsNoTracking()
            .Where(transfer =>
                transfer.UserId == userId &&
                transfer.Status == TransactionStatus.Completed &&
                transfer.TransferDate >= periodStart &&
                transfer.TransferDate <= periodEnd);

        if (accountId.HasValue)
            periodTransfersQuery = periodTransfersQuery.Where(transfer =>
                transfer.FromAccountId == accountId.Value || transfer.ToAccountId == accountId.Value);

        var periodTransfers = await periodTransfersQuery
            .Select(transfer => new
            {
                transfer.Id,
                transfer.FromAccountId,
                FromAccountName = transfer.FromAccount.Name,
                transfer.ToAccountId,
                ToAccountName = transfer.ToAccount.Name,
                transfer.Amount,
                transfer.TransferDate,
                transfer.Description,
                transfer.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var openingTransactions = await db.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.UserId == userId &&
                transaction.Status == TransactionStatus.Completed &&
                transaction.TransactionDate < startDate &&
                (!accountId.HasValue || transaction.AccountId == accountId.Value))
            .SumAsync(transaction => transaction.TransactionType == TransactionType.Income
                ? transaction.Amount
                : -transaction.Amount, cancellationToken);

        decimal openingTransfers = 0;
        if (accountId.HasValue)
        {
            openingTransfers = await db.Transfers
                .AsNoTracking()
                .Where(transfer =>
                    transfer.UserId == userId &&
                    transfer.Status == TransactionStatus.Completed &&
                    transfer.TransferDate < periodStart &&
                    (transfer.FromAccountId == accountId.Value || transfer.ToAccountId == accountId.Value))
                .SumAsync(transfer => transfer.ToAccountId == accountId.Value ? transfer.Amount : -transfer.Amount, cancellationToken);
        }

        var selectedAccountEntities = accountEntities
            .Where(account => !accountId.HasValue || account.Id == accountId.Value)
            .ToList();
        var openingBalanceEntries = selectedAccountEntities
            .Where(account =>
                account.InitialBalance != 0 &&
                account.CreatedAt >= periodStart &&
                account.CreatedAt <= periodEnd)
            .ToList();
        var openingBalance = selectedAccountEntities
            .Where(account => account.CreatedAt < periodStart)
            .Sum(account => account.InitialBalance) + openingTransactions + openingTransfers;

        var transactionByDate = periodTransactions
            .GroupBy(transaction => transaction.TransactionDate)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Income = group.Where(item => item.TransactionType == TransactionType.Income).Sum(item => item.Amount),
                    Expense = group.Where(item => item.TransactionType == TransactionType.Expense).Sum(item => item.Amount)
                });

        var transferByDate = periodTransfers
            .GroupBy(transfer => DateOnly.FromDateTime(transfer.TransferDate))
            .ToDictionary(
                group => group.Key,
                group => accountId.HasValue
                    ? group.Sum(item => item.ToAccountId == accountId.Value ? item.Amount : -item.Amount)
                    : 0m);

        var openingBalanceByDate = openingBalanceEntries
            .GroupBy(account => DateOnly.FromDateTime(account.CreatedAt))
            .ToDictionary(group => group.Key, group => group.Sum(account => account.InitialBalance));

        var chart = new List<FinanceChartPointResponse>();
        var runningBalance = openingBalance;
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dailyTransactions = transactionByDate.GetValueOrDefault(date);
            var income = dailyTransactions?.Income ?? 0;
            var expense = dailyTransactions?.Expense ?? 0;
            var net = income - expense +
                      transferByDate.GetValueOrDefault(date) +
                      openingBalanceByDate.GetValueOrDefault(date);
            runningBalance += net;
            chart.Add(new FinanceChartPointResponse(date, income, expense, net, runningBalance));
        }

        var movements = periodTransactions
            .Select(transaction => new FinanceMovementResponse(
                $"transaction-{transaction.Id}",
                transaction.TransactionType == TransactionType.Income ? "income" : "expense",
                transaction.Description,
                transaction.Amount,
                transaction.TransactionDate.ToDateTime(new TimeOnly(12, 0)),
                transaction.CreatedAt,
                transaction.AccountId,
                transaction.AccountName,
                null,
                transaction.CategoryName,
                transaction.CategoryColor))
            .Concat(periodTransfers.Select(transfer => new FinanceMovementResponse(
                $"transfer-{transfer.Id}",
                "transfer",
                string.IsNullOrWhiteSpace(transfer.Description) ? "Transferência entre contas" : transfer.Description,
                transfer.Amount,
                transfer.TransferDate,
                transfer.CreatedAt,
                transfer.FromAccountId,
                transfer.FromAccountName,
                transfer.ToAccountName,
                null,
                null)))
            .Concat(openingBalanceEntries.Select(account => new FinanceMovementResponse(
                $"opening-balance-{account.Id}",
                "opening_balance",
                "Saldo inicial",
                account.InitialBalance,
                account.CreatedAt,
                account.CreatedAt,
                account.Id,
                account.Name,
                null,
                null,
                null)))
            .OrderByDescending(movement => movement.Date.Date)
            .ThenByDescending(movement => movement.CreatedAt)
            .Take(25)
            .ToList();

        var categories = await db.FinancialCategories
            .AsNoTracking()
            .Where(category => category.UserId == userId && category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new FinanceCategoryResponse(
                category.Id,
                category.Name,
                category.CategoryType,
                category.Color))
            .ToListAsync(cancellationToken);

        var recurringTransactions = await db.RecurringTransactions
            .AsNoTracking()
            .Where(recurrence => recurrence.UserId == userId)
            .OrderByDescending(recurrence => recurrence.IsActive)
            .ThenBy(recurrence => recurrence.NextDueDate)
            .Select(recurrence => new FinanceRecurringResponse(
                recurrence.Id,
                recurrence.Description,
                recurrence.Amount,
                recurrence.Frequency,
                recurrence.NextDueDate,
                recurrence.EndDate,
                recurrence.IsActive,
                recurrence.AccountId,
                recurrence.Account.Name,
                recurrence.CategoryId,
                recurrence.Category == null ? null : recurrence.Category.Name,
                recurrence.Category == null ? null : recurrence.Category.Color))
            .ToListAsync(cancellationToken);

        var incomeTotal = periodTransactions
            .Where(transaction => transaction.TransactionType == TransactionType.Income)
            .Sum(transaction => transaction.Amount);
        var expenseTotal = periodTransactions
            .Where(transaction => transaction.TransactionType == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);
        var incomingTransfers = accountId.HasValue
            ? periodTransfers.Where(transfer => transfer.ToAccountId == accountId.Value).Sum(transfer => transfer.Amount)
            : 0;
        var outgoingTransfers = accountId.HasValue
            ? periodTransfers.Where(transfer => transfer.FromAccountId == accountId.Value).Sum(transfer => transfer.Amount)
            : 0;
        var selectedBalance = accounts
            .Where(account => !accountId.HasValue || account.Id == accountId.Value)
            .Sum(account => account.Balance);

        return Ok(new FinanceDashboardResponse(
            startDate,
            endDate,
            accountId,
            new FinanceSummaryResponse(
                selectedBalance,
                incomeTotal,
                expenseTotal,
                incomeTotal - expenseTotal,
                incomingTransfers,
                outgoingTransfers),
            accounts,
            chart,
            movements,
            recurringTransactions,
            categories));
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount(
        CreateFinanceAccountRequest request,
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
        return Created($"/api/finance/accounts/{account.Id}", new { account.Id });
    }

    [HttpPut("accounts/{id:long}")]
    public async Task<IActionResult> UpdateAccount(
        ulong id,
        UpdateFinanceAccountRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var account = await db.Accounts.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == userId && item.IsActive,
            cancellationToken);
        if (account is null) return NotFound();

        var name = request.Name.Trim();
        var duplicatedName = await db.Accounts.AnyAsync(
            item => item.Id != id && item.UserId == userId && item.Name == name && item.IsActive,
            cancellationToken);
        if (duplicatedName)
            return Conflict(new ProblemDetails { Title = "Você já possui uma conta com esse nome." });

        account.Name = name;
        account.AccountType = request.AccountType;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("accounts/{id:long}")]
    public async Task<IActionResult> DeleteAccount(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var account = await db.Accounts.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == userId && item.IsActive,
            cancellationToken);
        if (account is null) return NotFound();

        var hasTransactions = await db.Transactions.AnyAsync(
            transaction => transaction.AccountId == id,
            cancellationToken);
        var hasTransfers = await db.Transfers.AnyAsync(
            transfer => transfer.FromAccountId == id || transfer.ToAccountId == id,
            cancellationToken);
        var hasRecurrences = await db.RecurringTransactions.AnyAsync(
            recurrence => recurrence.AccountId == id,
            cancellationToken);

        if (hasTransactions || hasTransfers || hasRecurrences)
            return Conflict(new ProblemDetails
            {
                Title = "Essa conta possui movimentações e não pode ser excluída.",
                Detail = "Você ainda pode editar o nome e o tipo da conta."
            });

        db.Accounts.Remove(account);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction(
        CreateFinanceTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var accountIsValid = await db.Accounts.AnyAsync(
            account => account.Id == request.AccountId && account.UserId == userId && account.IsActive,
            cancellationToken);
        if (!accountIsValid)
            return BadRequest(new ProblemDetails { Title = "Selecione uma conta válida." });

        if (request.CategoryId.HasValue)
        {
            var categoryIsValid = await db.FinancialCategories.AnyAsync(
                category => category.Id == request.CategoryId &&
                            category.UserId == userId &&
                            category.IsActive &&
                            category.CategoryType == request.TransactionType,
                cancellationToken);
            if (!categoryIsValid)
                return BadRequest(new ProblemDetails { Title = "A categoria não combina com o tipo de movimentação." });
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
        return Created($"/api/finance/transactions/{transaction.Id}", new { transaction.Id });
    }

    [HttpPost("categories")]
    public async Task<ActionResult<FinanceCategoryResponse>> CreateCategory(
        CreateFinanceCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var name = request.Name.Trim();

        var duplicated = await db.FinancialCategories.AnyAsync(
            category => category.UserId == userId &&
                        category.Name == name &&
                        category.CategoryType == request.CategoryType,
            cancellationToken);
        if (duplicated)
            return Conflict(new ProblemDetails { Title = "Você já possui essa categoria para esse tipo de movimentação." });

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

        return Ok(new FinanceCategoryResponse(
            category.Id,
            category.Name,
            category.CategoryType,
            category.Color));
    }

    [HttpPost("recurring-transactions")]
    public async Task<ActionResult<FinanceRecurringResponse>> CreateRecurringTransaction(
        CreateFinanceRecurringRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (request.FirstDueDate < today)
            return BadRequest(new ProblemDetails
            {
                Title = "O primeiro vencimento não pode estar no passado.",
                Detail = "Registre despesas antigas como movimentações comuns."
            });
        if (request.EndDate < request.FirstDueDate)
            return BadRequest(new ProblemDetails { Title = "A data final não pode ser anterior ao primeiro vencimento." });

        var account = await db.Accounts.SingleOrDefaultAsync(
            item => item.Id == request.AccountId && item.UserId == userId && item.IsActive,
            cancellationToken);
        if (account is null)
            return BadRequest(new ProblemDetails { Title = "Selecione uma conta válida." });

        FinancialCategory? category = null;
        if (request.CategoryId.HasValue)
        {
            category = await db.FinancialCategories.SingleOrDefaultAsync(
                item => item.Id == request.CategoryId &&
                        item.UserId == userId &&
                        item.IsActive &&
                        item.CategoryType == TransactionType.Expense,
                cancellationToken);
            if (category is null)
                return BadRequest(new ProblemDetails { Title = "Selecione uma categoria de saída válida." });
        }

        var recurrence = new RecurringTransaction
        {
            UserId = userId,
            AccountId = request.AccountId,
            CategoryId = request.CategoryId,
            TransactionType = TransactionType.Expense,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Frequency = request.Frequency,
            StartDate = request.FirstDueDate,
            NextDueDate = request.FirstDueDate,
            EndDate = request.EndDate,
            IsActive = true
        };
        db.RecurringTransactions.Add(recurrence);
        await db.SaveChangesAsync(cancellationToken);
        await recurringProcessor.ProcessUserDueAsync(userId, cancellationToken);

        return Ok(new FinanceRecurringResponse(
            recurrence.Id,
            recurrence.Description,
            recurrence.Amount,
            recurrence.Frequency,
            recurrence.NextDueDate,
            recurrence.EndDate,
            recurrence.IsActive,
            recurrence.AccountId,
            account.Name,
            recurrence.CategoryId,
            category?.Name,
            category?.Color));
    }

    [HttpPatch("recurring-transactions/{id:long}/pause")]
    public async Task<IActionResult> PauseRecurringTransaction(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var recurrence = await db.RecurringTransactions.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == userId,
            cancellationToken);
        if (recurrence is null) return NotFound();

        recurrence.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPatch("recurring-transactions/{id:long}/resume")]
    public async Task<IActionResult> ResumeRecurringTransaction(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var recurrence = await db.RecurringTransactions.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == userId,
            cancellationToken);
        if (recurrence is null) return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);
        while (recurrence.NextDueDate < today)
            recurrence.NextDueDate = RecurringTransactionProcessor.GetNextDueDate(
                recurrence,
                recurrence.NextDueDate);

        if (recurrence.EndDate.HasValue && recurrence.NextDueDate > recurrence.EndDate.Value)
            return Conflict(new ProblemDetails { Title = "Essa recorrência já chegou à data final." });

        recurrence.IsActive = true;
        await db.SaveChangesAsync(cancellationToken);
        await recurringProcessor.ProcessUserDueAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("transfers")]
    public async Task<IActionResult> CreateTransfer(
        CreateFinanceTransferRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        if (request.FromAccountId == request.ToAccountId)
            return BadRequest(new ProblemDetails { Title = "Escolha contas diferentes para a transferência." });

        await using var databaseTransaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var accounts = await db.Accounts
            .Where(account => account.UserId == userId && account.IsActive &&
                              (account.Id == request.FromAccountId || account.Id == request.ToAccountId))
            .ToListAsync(cancellationToken);
        if (accounts.Count != 2)
            return BadRequest(new ProblemDetails { Title = "As duas contas precisam pertencer a você." });

        var originAccount = accounts.Single(account => account.Id == request.FromAccountId);
        var transactionBalance = await db.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.AccountId == request.FromAccountId &&
                transaction.Status == TransactionStatus.Completed)
            .SumAsync(transaction => transaction.TransactionType == TransactionType.Income
                ? transaction.Amount
                : -transaction.Amount, cancellationToken);
        var incomingBalance = await db.Transfers
            .AsNoTracking()
            .Where(transfer =>
                transfer.ToAccountId == request.FromAccountId &&
                transfer.Status == TransactionStatus.Completed)
            .SumAsync(transfer => transfer.Amount, cancellationToken);
        var outgoingBalance = await db.Transfers
            .AsNoTracking()
            .Where(transfer =>
                transfer.FromAccountId == request.FromAccountId &&
                transfer.Status == TransactionStatus.Completed)
            .SumAsync(transfer => transfer.Amount, cancellationToken);
        var availableBalance = originAccount.InitialBalance + transactionBalance + incomingBalance - outgoingBalance;

        if (request.Amount > availableBalance)
            return UnprocessableEntity(new ProblemDetails
            {
                Title = "Saldo insuficiente para essa transferência.",
                Detail = $"A conta possui {availableBalance:C} disponíveis."
            });

        var transfer = new Transfer
        {
            UserId = userId,
            FromAccountId = request.FromAccountId,
            ToAccountId = request.ToAccountId,
            Amount = request.Amount,
            TransferDate = request.TransferDate,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Status = TransactionStatus.Completed
        };
        db.Transfers.Add(transfer);
        await db.SaveChangesAsync(cancellationToken);
        await databaseTransaction.CommitAsync(cancellationToken);
        return Created($"/api/finance/transfers/{transfer.Id}", new { transfer.Id });
    }

    private bool TryGetUserId(out ulong userId)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return ulong.TryParse(subject, out userId);
    }
}
