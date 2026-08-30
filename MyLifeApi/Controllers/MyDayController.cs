using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs.MyDay;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;

namespace MyLifeApi.Controllers;

[ApiController]
[Authorize]
[Route("api/my-day")]
public sealed class MyDayController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<MyDayResponse>> Get(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Now);
        var activities = await db.Activities
            .AsNoTracking()
            .Where(activity => activity.UserId == userId && activity.ActivityDate == selectedDate)
            .OrderBy(activity => activity.StartedAt == null)
            .ThenBy(activity => activity.StartedAt)
            .ThenBy(activity => activity.CreatedAt)
            .Select(activity => new MyDayActivityResponse(
                activity.Id,
                activity.Title,
                activity.Description,
                activity.ActivityDate,
                activity.StartedAt,
                activity.EndedAt,
                activity.DurationSeconds,
                activity.Status == ActivityStatus.Planned ? "planned" :
                activity.Status == ActivityStatus.InProgress ? "in_progress" :
                activity.Status == ActivityStatus.Completed ? "completed" : "cancelled",
                activity.Status == ActivityStatus.Planned &&
                activity.Source == ActivitySource.Timer &&
                activity.DurationSeconds > 0,
                activity.Category == null ? null : activity.Category.Name,
                activity.Category == null ? null : activity.Category.Color))
            .ToListAsync(cancellationToken);

        var categories = await db.Categories
            .AsNoTracking()
            .Where(category => category.UserId == userId && category.IsActive)
            .OrderBy(category => category.Name)
            .Select(category => new MyDayCategoryResponse(
                category.Id,
                category.Name,
                category.Color,
                category.Icon))
            .ToListAsync(cancellationToken);

        var completed = activities.Count(activity => activity.Status == "completed");
        var total = activities.Count;
        var summary = new MyDaySummaryResponse(
            total,
            completed,
            activities.Count(activity => activity.Status == "planned"),
            activities.Count(activity => activity.Status == "in_progress"),
            (uint)activities
            .Where(activity => activity.Status is "completed" or "in_progress")
                .Sum(activity => (long)(activity.DurationSeconds ?? 0)),
            total == 0 ? 0 : Math.Round((decimal)completed / total * 100, 1));

        return Ok(new MyDayResponse(selectedDate, summary, activities, categories));
    }

    [HttpPost("activities")]
    public async Task<ActionResult<MyDayActivityResponse>> Create(
        CreateMyDayActivityRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();

        if (request.CategoryId.HasValue &&
            !await db.Categories.AnyAsync(
                category => category.Id == request.CategoryId &&
                            category.UserId == userId &&
                            category.IsActive,
                cancellationToken))
        {
            return BadRequest(new ProblemDetails { Title = "Selecione uma categoria válida." });
        }

        var activity = new Activity
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description)
                ? null
                : request.Description.Trim(),
            ActivityDate = request.ActivityDate,
            StartedAt = request.StartTime.HasValue
                ? request.ActivityDate.ToDateTime(request.StartTime.Value)
                : null,
            DurationSeconds = request.DurationMinutes.HasValue
                ? checked((uint)request.DurationMinutes.Value * 60)
                : null,
            Status = ActivityStatus.Planned,
            Source = ActivitySource.Manual
        };

        db.Activities.Add(activity);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { date = activity.ActivityDate },
            await ToResponse(activity.Id, userId, cancellationToken));
    }

    [HttpPatch("activities/{id:long}/complete")]
    public async Task<ActionResult<MyDayActivityResponse>> Complete(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var activity = await FindActivity(id, userId, cancellationToken);
        if (activity is null) return NotFound();

        var now = DateTime.Now;
        if (!activity.StartedAt.HasValue || activity.StartedAt > now)
        {
            activity.StartedAt = activity.DurationSeconds.HasValue
                ? now.AddSeconds(-activity.DurationSeconds.Value)
                : now;
        }

        activity.EndedAt = now;
        activity.DurationSeconds ??= (uint)Math.Max(0, (now - activity.StartedAt.Value).TotalSeconds);
        activity.Status = ActivityStatus.Completed;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(await ToResponse(activity.Id, userId, cancellationToken));
    }

    [HttpPatch("activities/{id:long}/start")]
    public async Task<ActionResult<MyDayActivityResponse>> Start(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var activity = await FindActivity(id, userId, cancellationToken);
        if (activity is null) return NotFound();
        if (activity.Status == ActivityStatus.Completed)
            return BadRequest(new ProblemDetails { Title = "Uma atividade concluída não pode ser iniciada." });

        var anotherTimerIsRunning = await db.Activities.AnyAsync(
            candidate => candidate.UserId == userId &&
                         candidate.Id != id &&
                         candidate.Status == ActivityStatus.InProgress,
            cancellationToken);

        if (anotherTimerIsRunning)
            return Conflict(new ProblemDetails { Title = "Finalize o cronômetro atual antes de iniciar outro." });

        if (activity.Source != ActivitySource.Timer)
            activity.DurationSeconds = 0;

        activity.StartedAt = DateTime.Now;
        activity.EndedAt = null;
        activity.Status = ActivityStatus.InProgress;
        activity.Source = ActivitySource.Timer;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(await ToResponse(activity.Id, userId, cancellationToken));
    }

    [HttpPatch("activities/{id:long}/pause")]
    public async Task<ActionResult<MyDayActivityResponse>> Pause(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var activity = await FindActivity(id, userId, cancellationToken);
        if (activity is null) return NotFound();
        if (activity.Status != ActivityStatus.InProgress || !activity.StartedAt.HasValue)
            return BadRequest(new ProblemDetails { Title = "Esta atividade não possui um cronômetro para pausar." });

        var now = DateTime.Now;
        var elapsedSeconds = (uint)Math.Max(0, (now - activity.StartedAt.Value).TotalSeconds);
        activity.DurationSeconds = checked((activity.DurationSeconds ?? 0) + elapsedSeconds);
        activity.StartedAt = null;
        activity.EndedAt = null;
        activity.Status = ActivityStatus.Planned;
        activity.Source = ActivitySource.Timer;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(await ToResponse(activity.Id, userId, cancellationToken));
    }

    [HttpPatch("activities/{id:long}/stop")]
    public async Task<ActionResult<MyDayActivityResponse>> Stop(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var activity = await FindActivity(id, userId, cancellationToken);
        if (activity is null) return NotFound();
        if (activity.Status != ActivityStatus.InProgress || !activity.StartedAt.HasValue)
            return BadRequest(new ProblemDetails { Title = "Esta atividade não possui um cronômetro ativo." });

        activity.EndedAt = DateTime.Now;
        var elapsedSeconds = (uint)Math.Max(
            0,
            (activity.EndedAt.Value - activity.StartedAt.Value).TotalSeconds);
        activity.DurationSeconds = checked((activity.DurationSeconds ?? 0) + elapsedSeconds);
        activity.Status = ActivityStatus.Completed;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(await ToResponse(activity.Id, userId, cancellationToken));
    }

    [HttpDelete("activities/{id:long}")]
    public async Task<IActionResult> Delete(
        ulong id,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId)) return Unauthorized();
        var activity = await FindActivity(id, userId, cancellationToken);
        if (activity is null) return NotFound();

        db.Activities.Remove(activity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private bool TryGetUserId(out ulong userId)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return ulong.TryParse(subject, out userId);
    }

    private Task<Activity?> FindActivity(
        ulong id,
        ulong userId,
        CancellationToken cancellationToken) =>
        db.Activities.SingleOrDefaultAsync(
            activity => activity.Id == id && activity.UserId == userId,
            cancellationToken);

    private async Task<MyDayActivityResponse> ToResponse(
        ulong id,
        ulong userId,
        CancellationToken cancellationToken) =>
        await db.Activities
            .AsNoTracking()
            .Where(activity => activity.Id == id && activity.UserId == userId)
            .Select(activity => new MyDayActivityResponse(
                activity.Id,
                activity.Title,
                activity.Description,
                activity.ActivityDate,
                activity.StartedAt,
                activity.EndedAt,
                activity.DurationSeconds,
                activity.Status == ActivityStatus.Planned ? "planned" :
                activity.Status == ActivityStatus.InProgress ? "in_progress" :
                activity.Status == ActivityStatus.Completed ? "completed" : "cancelled",
                activity.Status == ActivityStatus.Planned &&
                activity.Source == ActivitySource.Timer &&
                activity.DurationSeconds > 0,
                activity.Category == null ? null : activity.Category.Name,
                activity.Category == null ? null : activity.Category.Color))
            .SingleAsync(cancellationToken);
}
