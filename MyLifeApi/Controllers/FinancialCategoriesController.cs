using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.FinancialCategories;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/financial-categories")]
public class FinancialCategoriesController(MyLifeDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FinancialCategoryResponse>>> GetAll([FromQuery] ulong? userId, CancellationToken ct)
    {
        var query = db.FinancialCategories.AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(category => category.UserId == userId);
        }

        var categories = await query
            .OrderBy(category => category.Name)
            .Select(DtoProjections.FinancialCategory)
            .ToListAsync(ct);

        return Ok(categories);
    }
    [HttpGet("{id:long}")]
    public async Task<ActionResult<FinancialCategoryResponse>> GetById(ulong id, CancellationToken ct)
    {
        var item = await db.FinancialCategories.AsNoTracking()
            .Where(category => category.Id == id)
            .Select(DtoProjections.FinancialCategory)
            .FirstOrDefaultAsync(ct);

        return item is null ? NotFound() : Ok(item);
    }
    [HttpPost]
    public async Task<ActionResult<FinancialCategoryResponse>> Create(FinancialCategoryRequest r, CancellationToken ct)
    {
        var error = await Validate(r, null, ct);

        if (error is not null)
        {
            return error;
        }

        var item = new FinancialCategory();
        Apply(item, r);

        db.FinancialCategories.Add(item);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ToResponse(item));
    }
    [HttpPut("{id:long}")]
    public async Task<ActionResult<FinancialCategoryResponse>> Update(ulong id, FinancialCategoryRequest r, CancellationToken ct)
    {
        var item = await db.FinancialCategories.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        var error = await Validate(r, id, ct);

        if (error is not null)
        {
            return error;
        }

        Apply(item, r);
        await db.SaveChangesAsync(ct);

        return Ok(ToResponse(item));
    }
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken ct)
    {
        var item = await db.FinancialCategories.FindAsync([id], ct);

        if (item is null)
        {
            return NotFound();
        }

        db.FinancialCategories.Remove(item);
        await db.SaveChangesAsync(ct);

        return NoContent();
    }
    private async Task<ActionResult?> Validate(FinancialCategoryRequest r, ulong? id, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(user => user.Id == r.UserId, ct))
        {
            return BadRequest(new ProblemDetails { Title = "Usuário inválido." });
        }

        var alreadyExists = await db.FinancialCategories.AnyAsync(
            category => category.Id != id &&
                        category.UserId == r.UserId &&
                        category.Name == r.Name &&
                        category.CategoryType == r.CategoryType,
            ct);

        if (alreadyExists)
        {
            return Conflict(new ProblemDetails { Title = "Categoria financeira já existente." });
        }

        return null;
    }

    private static void Apply(FinancialCategory category, FinancialCategoryRequest request)
    {
        category.UserId = request.UserId;
        category.Name = request.Name.Trim();
        category.CategoryType = request.CategoryType;
        category.Icon = request.Icon;
        category.Color = request.Color;
        category.IsActive = request.IsActive;
    }

    private static FinancialCategoryResponse ToResponse(FinancialCategory category) => new(
        category.Id, category.UserId, category.Name, category.CategoryType,
        category.Icon, category.Color, category.IsActive,
        category.CreatedAt, category.UpdatedAt);
}
