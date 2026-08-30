using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.ActivityGoals;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/activity-goals")]
public class ActivityGoalsController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityGoalResponse>>> GetAll([FromQuery] ulong? activityId, [FromQuery] ulong? goalId, CancellationToken ct)
    {
        var query = db.ActivityGoals.AsNoTracking();

        if (activityId.HasValue)
        {
            query = query.Where(link => link.ActivityId == activityId);
        }

        if (goalId.HasValue)
        {
            query = query.Where(link => link.GoalId == goalId);
        }

        var links = await query.Select(DtoProjections.ActivityGoal).ToListAsync(ct);
        return Ok(links);
    }
    [HttpPost]
    public async Task<ActionResult<ActivityGoalResponse>> Create(CreateActivityGoalRequest request, CancellationToken ct)
    {
        if (await db.ActivityGoals.AnyAsync(
                link => link.ActivityId == request.ActivityId && link.GoalId == request.GoalId,
                ct))
        {
            return Conflict(new ProblemDetails { Title = "Vínculo já existente." });
        }

        var owners = await db.Activities.Where(x => x.Id == request.ActivityId).Select(x => (ulong?)x.UserId).FirstOrDefaultAsync(ct);
        var goalOwner = await db.Goals.Where(x => x.Id == request.GoalId).Select(x => (ulong?)x.UserId).FirstOrDefaultAsync(ct);
        if (!owners.HasValue || !goalOwner.HasValue)
        {
            return BadRequest(new ProblemDetails { Title = "Atividade ou meta inválida." });
        }

        if (owners != goalOwner)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Atividade e meta devem pertencer ao mesmo usuário."
            });
        }

        var item = new ActivityGoal
        {
            ActivityId = request.ActivityId,
            GoalId = request.GoalId
        };

        db.ActivityGoals.Add(item);
        await db.SaveChangesAsync(ct);

        return Created($"api/activity-goals/{item.ActivityId}/{item.GoalId}", new ActivityGoalResponse(item.ActivityId, item.GoalId, item.CreatedAt));
    }
    [HttpDelete("{activityId:long}/{goalId:long}")]
    public async Task<IActionResult> Delete(ulong activityId, ulong goalId, CancellationToken ct)
    {
        var item = await db.ActivityGoals.FindAsync([activityId, goalId], ct);

        if (item is null)
        {
            return NotFound();
        }

        db.ActivityGoals.Remove(item);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }
}
