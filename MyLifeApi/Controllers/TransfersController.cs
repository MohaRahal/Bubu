using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.Transfers;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/transfers")]
public class TransfersController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransferResponse>>> GetAll(
        [FromQuery] ulong? userId,
        CancellationToken cancellationToken)
    {
        var query = db.Transfers.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(transfer => transfer.UserId == userId);
        }

        var transfers = await query
            .OrderByDescending(transfer => transfer.TransferDate)
            .Select(DtoProjections.Transfer)
            .ToListAsync(cancellationToken);

        return Ok(transfers);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TransferResponse>> GetById(
        ulong id,
        CancellationToken cancellationToken)
    {
        var transfer = await db.Transfers
            .AsNoTracking()
            .Where(transfer => transfer.Id == id)
            .Select(DtoProjections.Transfer)
            .FirstOrDefaultAsync(cancellationToken);

        return transfer is null ? NotFound() : Ok(transfer);
    }

    [HttpPost]
    public async Task<ActionResult<TransferResponse>> Create(
        TransferRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await Validate(request, cancellationToken);

        if (validationError is not null)
        {
            return BadRequest(new ProblemDetails { Title = validationError });
        }

        var transfer = new Transfer();
        Apply(transfer, request);

        db.Transfers.Add(transfer);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = transfer.Id },
            ToResponse(transfer));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<TransferResponse>> Update(
        ulong id,
        TransferRequest request,
        CancellationToken cancellationToken)
    {
        var transfer = await db.Transfers.FindAsync([id], cancellationToken);

        if (transfer is null)
        {
            return NotFound();
        }

        var validationError = await Validate(request, cancellationToken);

        if (validationError is not null)
        {
            return BadRequest(new ProblemDetails { Title = validationError });
        }

        Apply(transfer, request);
        await db.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(transfer));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var transfer = await db.Transfers.FindAsync([id], cancellationToken);

        if (transfer is null)
        {
            return NotFound();
        }

        db.Transfers.Remove(transfer);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<string?> Validate(
        TransferRequest request,
        CancellationToken cancellationToken)
    {
        if (request.FromAccountId == request.ToAccountId)
        {
            return "As contas de origem e destino devem ser diferentes.";
        }

        var validAccountsCount = await db.Accounts.CountAsync(
            account => account.UserId == request.UserId &&
                       (account.Id == request.FromAccountId ||
                        account.Id == request.ToAccountId),
            cancellationToken);

        return validAccountsCount == 2
            ? null
            : "As duas contas devem pertencer ao usuário.";
    }

    private static void Apply(Transfer transfer, TransferRequest request)
    {
        transfer.UserId = request.UserId;
        transfer.FromAccountId = request.FromAccountId;
        transfer.ToAccountId = request.ToAccountId;
        transfer.Amount = request.Amount;
        transfer.TransferDate = request.TransferDate;
        transfer.Description = request.Description;
        transfer.Status = request.Status;
    }

    private static TransferResponse ToResponse(Transfer transfer) => new(
        transfer.Id,
        transfer.UserId,
        transfer.FromAccountId,
        transfer.ToAccountId,
        transfer.Amount,
        transfer.TransferDate,
        transfer.Description,
        transfer.Status,
        transfer.CreatedAt,
        transfer.UpdatedAt);
}
