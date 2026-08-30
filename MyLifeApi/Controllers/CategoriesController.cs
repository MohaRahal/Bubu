using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.Categories;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll([FromQuery] ulong? userId, CancellationToken ct)
    {
        var query = db.Categories.AsNoTracking();
        if (userId.HasValue)
        {
            query = query.Where(category => category.UserId == userId);
        }

        return Ok(await query.OrderBy(category => category.Name).Select(DtoProjections.Category).ToListAsync(ct));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CategoryResponse>> GetById(ulong id, CancellationToken ct)
    {
        var item = await db.Categories.AsNoTracking().Where(category => category.Id == id).Select(DtoProjections.Category).FirstOrDefaultAsync(ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CategoryRequest request, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(user => user.Id == request.UserId, ct))
        {
            return BadRequest(new ProblemDetails { Title = "Usuário inválido." });
        }

        if (await db.Categories.AnyAsync(
                category => category.UserId == request.UserId && category.Name == request.Name,
                ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "O usuário já possui uma categoria com esse nome."
            });
        }

        var item = new Category { UserId = request.UserId, Name = request.Name.Trim(), Description = request.Description, Icon = request.Icon, Color = request.Color, IsActive = request.IsActive };

        db.Categories.Add(item);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponse(item));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<CategoryResponse>> Update(ulong id, CategoryRequest request, CancellationToken ct)
    {
        var item = await db.Categories.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        if (!await db.Users.AnyAsync(user => user.Id == request.UserId, ct))
        {
            return BadRequest(new ProblemDetails { Title = "Usuário inválido." });
        }

        if (await db.Categories.AnyAsync(
                category => category.Id != id &&
                            category.UserId == request.UserId &&
                            category.Name == request.Name,
                ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "O usuário já possui uma categoria com esse nome."
            });
        }

        item.UserId = request.UserId;
        item.Name = request.Name.Trim();
        item.Description = request.Description;
        item.Icon = request.Icon;
        item.Color = request.Color;
        item.IsActive = request.IsActive;

        await db.SaveChangesAsync(ct);

        return Ok(ToResponse(item));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        var item = await db.Categories.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        db.Categories.Remove(item);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }

    private static CategoryResponse ToResponse(Category item) => new(item.Id, item.UserId, item.Name, item.Description, item.Icon, item.Color, item.IsActive, item.CreatedAt, item.UpdatedAt);
}
