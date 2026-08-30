using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.Accounts;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/accounts")]
public class AccountsController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AccountResponse>>> GetAll([FromQuery] ulong? userId, CancellationToken ct)
    {
        var query = db.Accounts.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(account => account.UserId == userId);
        }

        var accounts = await query
            .OrderBy(account => account.Name)
            .Select(DtoProjections.Account)
            .ToListAsync(ct);

        return Ok(accounts);
    }
    [HttpGet("{id:long}")]
    public async Task<ActionResult<AccountResponse>> GetById(ulong id, CancellationToken ct)
    {
        var item = await db.Accounts.AsNoTracking()
            .Where(account => account.Id == id)
            .Select(DtoProjections.Account)
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<ActionResult<AccountResponse>> Create(AccountRequest r, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(user => user.Id == r.UserId, ct))
        {
            return BadRequest(new ProblemDetails { Title = "Usuário inválido." });
        }

        var item = new Account();
        Apply(item, r);

        db.Accounts.Add(item);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponse(item));
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<AccountResponse>> Update(ulong id, AccountRequest r, CancellationToken ct)
    {
        var item = await db.Accounts.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        if (!await db.Users.AnyAsync(user => user.Id == r.UserId, ct))
        {
            return BadRequest(new ProblemDetails { Title = "Usuário inválido." });
        }

        Apply(item, r);
        await db.SaveChangesAsync(ct);

        return Ok(ToResponse(item));
    }
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        var item = await db.Accounts.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        var hasMovements =
            await db.Transactions.AnyAsync(transaction => transaction.AccountId == id, ct) ||
            await db.RecurringTransactions.AnyAsync(transaction => transaction.AccountId == id, ct) ||
            await db.Transfers.AnyAsync(
                transfer => transfer.FromAccountId == id || transfer.ToAccountId == id,
                ct);

        if (hasMovements)
        {
            return Conflict(new ProblemDetails
            {
                Title = "A conta possui movimentações e não pode ser excluída."
            });
        }

        db.Accounts.Remove(item);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static void Apply(Account account, AccountRequest request)
    {
        account.UserId = request.UserId;
        account.Name = request.Name.Trim();
        account.AccountType = request.AccountType;
        account.InitialBalance = request.InitialBalance;
        account.IsActive = request.IsActive;
    }

    private static AccountResponse ToResponse(Account account) => new(
        account.Id, account.UserId, account.Name, account.AccountType,
        account.InitialBalance, account.IsActive,
        account.CreatedAt, account.UpdatedAt);
}
