using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.Activities;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/activities")]
public class ActivitiesController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityResponse>>> GetAll([FromQuery] ulong? userId, CancellationToken ct)
    {
        var query = db.Activities.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(activity => activity.UserId == userId);
        }

        return Ok(await query.OrderByDescending(x => x.ActivityDate).Select(DtoProjections.Activity).ToListAsync(ct));
    }
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ActivityResponse>> GetById(ulong id, CancellationToken ct)
    {
        var item = await db.Activities
            .AsNoTracking()
            .Where(activity => activity.Id == id)
            .Select(DtoProjections.Activity)
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<ActionResult<ActivityResponse>> Create(ActivityRequest request, CancellationToken ct)
    {
        var error = await Validate(request, ct);

        if (error is not null)
        {
            return BadRequest(new ProblemDetails { Title = error });
        }

        var item = new Activity();
        Apply(item, request);

        db.Activities.Add(item);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponse(item));
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ActivityResponse>> Update(ulong id, ActivityRequest request, CancellationToken ct)
    {
        var item = await db.Activities.FindAsync([id], ct);

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
        var item = await db.Activities.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        db.Activities.Remove(item);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private async Task<string?> Validate(ActivityRequest r, CancellationToken ct)
    {
        if (r.EndedAt < r.StartedAt)
        {
            return "A data final não pode ser anterior à inicial.";
        }

        if (!await db.Users.AnyAsync(user => user.Id == r.UserId, ct))
        {
            return "Usuário inválido.";
        }

        if (r.CategoryId.HasValue &&
            !await db.Categories.AnyAsync(
                category => category.Id == r.CategoryId && category.UserId == r.UserId,
                ct))
        {
            return "A categoria não pertence ao usuário.";
        }

        return null;
    }

    private static void Apply(Activity activity, ActivityRequest request)
    {
        activity.UserId = request.UserId;
        activity.CategoryId = request.CategoryId;
        activity.Title = request.Title.Trim();
        activity.Description = request.Description;
        activity.ActivityDate = request.ActivityDate;
        activity.StartedAt = request.StartedAt;
        activity.EndedAt = request.EndedAt;
        activity.DurationSeconds = request.DurationSeconds;
        activity.Status = request.Status;
        activity.Source = request.Source;
    }

    private static ActivityResponse ToResponse(Activity activity) => new(
        activity.Id,
        activity.UserId,
        activity.CategoryId,
        activity.Title,
        activity.Description,
        activity.ActivityDate,
        activity.StartedAt,
        activity.EndedAt,
        activity.DurationSeconds,
        activity.Status,
        activity.Source,
        activity.CreatedAt,
        activity.UpdatedAt);
}
