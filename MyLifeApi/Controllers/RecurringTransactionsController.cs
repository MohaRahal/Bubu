using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.RecurringTransactions;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/recurring-transactions")]
public class RecurringTransactionsController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecurringTransactionResponse>>> GetAll(
        [FromQuery] ulong? userId,
        CancellationToken cancellationToken)
    {
        var query = db.RecurringTransactions.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(transaction => transaction.UserId == userId);
        }

        var transactions = await query
            .OrderBy(transaction => transaction.NextDueDate)
            .Select(DtoProjections.RecurringTransaction)
            .ToListAsync(cancellationToken);

        return Ok(transactions);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<RecurringTransactionResponse>> GetById(
        ulong id,
        CancellationToken cancellationToken)
    {
        var transaction = await db.RecurringTransactions
            .AsNoTracking()
            .Where(transaction => transaction.Id == id)
            .Select(DtoProjections.RecurringTransaction)
            .FirstOrDefaultAsync(cancellationToken);

        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<RecurringTransactionResponse>> Create(
        RecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await Validate(request, cancellationToken);

        if (validationError is not null)
        {
            return BadRequest(new ProblemDetails { Title = validationError });
        }

        var transaction = new RecurringTransaction();
        Apply(transaction, request);

        db.RecurringTransactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transaction.Id },
            ToResponse(transaction));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<RecurringTransactionResponse>> Update(
        ulong id,
        RecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await db.RecurringTransactions.FindAsync([id], cancellationToken);

        if (transaction is null)
        {
            return NotFound();
        }

        var validationError = await Validate(request, cancellationToken);

        if (validationError is not null)
        {
            return BadRequest(new ProblemDetails { Title = validationError });
        }

        Apply(transaction, request);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(transaction));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var transaction = await db.RecurringTransactions.FindAsync([id], cancellationToken);

        if (transaction is null)
        {
            return NotFound();
        }

        db.RecurringTransactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<string?> Validate(
        RecurringTransactionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EndDate < request.StartDate)
        {
            return "A data final não pode ser anterior à data inicial.";
        }

        if (request.NextDueDate < request.StartDate)
        {
            return "O próximo vencimento não pode ser anterior à data inicial.";
        }

        var accountIsValid = await db.Accounts.AnyAsync(
            account => account.Id == request.AccountId && account.UserId == request.UserId,
            cancellationToken);

        if (!accountIsValid)
        {
            return "A conta não pertence ao usuário.";
        }

        if (request.CategoryId.HasValue)
        {
            var categoryIsValid = await db.FinancialCategories.AnyAsync(
                category => category.Id == request.CategoryId &&
                            category.UserId == request.UserId &&
                            category.CategoryType == request.TransactionType,
                cancellationToken);

            if (!categoryIsValid)
            {
                return "A categoria não pertence ao usuário ou possui tipo incompatível.";
            }
        }

        return null;
    }

    private static void Apply(
        RecurringTransaction transaction,
        RecurringTransactionRequest request)
    {
        transaction.UserId = request.UserId;
        transaction.AccountId = request.AccountId;
        transaction.CategoryId = request.CategoryId;
        transaction.TransactionType = request.TransactionType;
        transaction.Description = request.Description.Trim();
        transaction.Amount = request.Amount;
        transaction.Frequency = request.Frequency;
        transaction.StartDate = request.StartDate;
        transaction.EndDate = request.EndDate;
        transaction.NextDueDate = request.NextDueDate;
        transaction.IsActive = request.IsActive;
    }

    private static RecurringTransactionResponse ToResponse(
        RecurringTransaction transaction) => new(
            transaction.Id,
            transaction.UserId,
            transaction.AccountId,
            transaction.CategoryId,
            transaction.TransactionType,
            transaction.Description,
            transaction.Amount,
            transaction.Frequency,
            transaction.StartDate,
            transaction.EndDate,
            transaction.NextDueDate,
            transaction.IsActive,
            transaction.CreatedAt,
            transaction.UpdatedAt);
}
