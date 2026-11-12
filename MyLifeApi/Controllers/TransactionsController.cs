using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.Transactions;
using MyLifeApi.Models;
using MyLifeApi.Services.Email;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionsController(
    MyLifeDbContext db,
    FinancialMovementEmailDispatcher emailDispatcher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransactionResponse>>> GetAll(
        [FromQuery] ulong? userId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var query = db.Transactions.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(transaction => transaction.UserId == userId);
        }

        if (from.HasValue)
        {
            query = query.Where(transaction => transaction.TransactionDate >= from);
        }

        if (to.HasValue)
        {
            query = query.Where(transaction => transaction.TransactionDate <= to);
        }

        var transactions = await query
            .OrderByDescending(transaction => transaction.TransactionDate)
            .Select(DtoProjections.Transaction)
            .ToListAsync(cancellationToken);

        return Ok(transactions);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TransactionResponse>> GetById(
        ulong id,
        CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.Id == id)
            .Select(DtoProjections.Transaction)
            .FirstOrDefaultAsync(cancellationToken);

        return transaction is null ? NotFound() : Ok(transaction);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> Create(
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await Validate(request, cancellationToken);

        if (validationError is not null)
        {
            return BadRequest(new ProblemDetails { Title = validationError });
        }

        var transaction = new Transaction();
        Apply(transaction, request);

        db.Transactions.Add(transaction);
        await db.SaveChangesAsync(cancellationToken);
        await emailDispatcher.NotifyAsync(transaction, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transaction.Id },
            ToResponse(transaction));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<TransactionResponse>> Update(
        ulong id,
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
        var transaction = await db.Transactions.FindAsync([id], cancellationToken);

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
        var transaction = await db.Transactions.FindAsync([id], cancellationToken);

        if (transaction is null)
        {
            return NotFound();
        }

        db.Transactions.Remove(transaction);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<string?> Validate(
        TransactionRequest request,
        CancellationToken cancellationToken)
    {
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

        if (request.RecurringTransactionId.HasValue)
        {
            var recurringIsValid = await db.RecurringTransactions.AnyAsync(
                recurring => recurring.Id == request.RecurringTransactionId &&
                             recurring.UserId == request.UserId &&
                             recurring.AccountId == request.AccountId &&
                             recurring.TransactionType == request.TransactionType,
                cancellationToken);

            if (!recurringIsValid)
            {
                return "A recorrência informada é incompatível com a transação.";
            }
        }

        return null;
    }

    private static void Apply(Transaction transaction, TransactionRequest request)
    {
        transaction.UserId = request.UserId;
        transaction.AccountId = request.AccountId;
        transaction.CategoryId = request.CategoryId;
        transaction.RecurringTransactionId = request.RecurringTransactionId;
        transaction.TransactionType = request.TransactionType;
        transaction.Description = request.Description.Trim();
        transaction.Amount = request.Amount;
        transaction.TransactionDate = request.TransactionDate;
        transaction.Status = request.Status;
        transaction.Notes = request.Notes;
    }

    private static TransactionResponse ToResponse(Transaction transaction) => new(
        transaction.Id,
        transaction.UserId,
        transaction.AccountId,
        transaction.CategoryId,
        transaction.RecurringTransactionId,
        transaction.TransactionType,
        transaction.Description,
        transaction.Amount,
        transaction.TransactionDate,
        transaction.Status,
        transaction.Notes,
        transaction.CreatedAt,
        transaction.UpdatedAt);
}
