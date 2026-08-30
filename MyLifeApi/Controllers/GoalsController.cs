using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.Goals;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/goals")]
public class GoalsController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GoalResponse>>> GetAll([FromQuery] ulong? userId, CancellationToken ct)
    {
        var query = db.Goals.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(goal => goal.UserId == userId);
        }

        var goals = await query
            .OrderBy(goal => goal.TargetDate)
            .Select(DtoProjections.Goal)
            .ToListAsync(ct);

        return Ok(goals);
    }
    [HttpGet("{id:long}")]
    public async Task<ActionResult<GoalResponse>> GetById(ulong id, CancellationToken ct)
    {
        var item = await db.Goals.AsNoTracking()
            .Where(goal => goal.Id == id)
            .Select(DtoProjections.Goal)
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<ActionResult<GoalResponse>> Create(GoalRequest request, CancellationToken ct)
    {
        var error = await Validate(request, ct);

        if (error is not null)
        {
            return BadRequest(new ProblemDetails { Title = error });
        }

        var item = new Goal();
        Apply(item, request);

        db.Goals.Add(item);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponse(item));
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<GoalResponse>> Update(ulong id, GoalRequest request, CancellationToken ct)
    {
        var item = await db.Goals.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        var error = await Validate(request, ct);

        if (error is not null)
        {
            return BadRequest(new ProblemDetails { Title = error });
        }

        Apply(item, request);
        await db.SaveChangesAsync(ct);

        return Ok(ToResponse(item));
    }
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        var item = await db.Goals.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        db.Goals.Remove(item);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<string?> Validate(GoalRequest r, CancellationToken ct)
    {
        if (r.TargetDate < r.StartDate)
        {
            return "A data-alvo não pode ser anterior à data inicial.";
        }

        return await db.Users.AnyAsync(user => user.Id == r.UserId, ct)
            ? null
            : "Usuário inválido.";
    }

    private static void Apply(Goal goal, GoalRequest request)
    {
        goal.UserId = request.UserId;
        goal.Title = request.Title.Trim();
        goal.Description = request.Description;
        goal.GoalType = request.GoalType;
        goal.TargetValue = request.TargetValue;
        goal.Unit = request.Unit;
        goal.StartDate = request.StartDate;
        goal.TargetDate = request.TargetDate;
        goal.Status = request.Status;
    }

    private static GoalResponse ToResponse(Goal goal) => new(
        goal.Id, goal.UserId, goal.Title, goal.Description,
        goal.GoalType, goal.TargetValue, goal.Unit,
        goal.StartDate, goal.TargetDate, goal.Status,
        goal.CreatedAt, goal.UpdatedAt);
}
