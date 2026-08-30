using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs;
using MyLifeApi.DTOs.Users;
using MyLifeApi.Models;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(MyLifeDbContext db, IPasswordHasher<User> passwordHasher) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await db.Users.AsNoTracking().OrderBy(user => user.Name)
            .Select(DtoProjections.User).ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var user = await db.Users.AsNoTracking().Where(user => user.Id == id)
            .Select(DtoProjections.User).FirstOrDefaultAsync(cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
            return Conflict(new ProblemDetails { Title = "E-mail já cadastrado." });

        var user = new User { Name = request.Name.Trim(), Email = email, PasswordHash = string.Empty, AvatarUrl = request.AvatarUrl, BirthDate = request.BirthDate };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, ToResponse(user));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UserResponse>> Update(ulong id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null) return NotFound();

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(other => other.Email == email && other.Id != id, cancellationToken))
            return Conflict(new ProblemDetails { Title = "E-mail já cadastrado." });

        user.Name = request.Name.Trim();
        user.Email = email;
        user.AvatarUrl = request.AvatarUrl;
        user.BirthDate = request.BirthDate;
        user.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(user));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null) return NotFound();
        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email, user.AvatarUrl, user.BirthDate, user.Status, user.LastLoginAt, user.CreatedAt, user.UpdatedAt);
}
