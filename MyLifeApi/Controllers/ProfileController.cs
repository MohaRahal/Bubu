using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyLifeApi.Data;
using MyLifeApi.DTOs.Profile;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;
using MyLifeApi.Services;

namespace MyLifeApi.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(
    MyLifeDbContext db,
    IPasswordHasher<User> passwordHasher,
    AvatarCatalogService avatarCatalog) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUser(cancellationToken);
        return user is null ? Unauthorized() : Ok(ToResponse(user));
    }

    [HttpPut]
    public async Task<ActionResult<ProfileResponse>> Update(
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUser(cancellationToken);
        if (user is null) return Unauthorized();

        var email = request.Email.Trim().ToLowerInvariant();
        var emailInUse = await db.Users.AnyAsync(
            candidate => candidate.Email == email && candidate.Id != user.Id,
            cancellationToken);

        if (emailInUse)
            return Conflict(new ProblemDetails { Title = "Este e-mail já está sendo utilizado." });

        if (request.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return BadRequest(new ProblemDetails { Title = "A data de nascimento não pode estar no futuro." });

        if (!avatarCatalog.TryResolve(request.AvatarKey, out var avatarPath))
            return BadRequest(new ProblemDetails { Title = "Selecione um avatar disponível no catálogo." });

        user.Name = request.Name.Trim();
        user.Email = email;
        user.AvatarUrl = avatarPath;
        user.BirthDate = request.BirthDate;

        await db.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(user));
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await GetCurrentUser(cancellationToken);
        if (user is null) return Unauthorized();

        var verification = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.CurrentPassword);

        if (verification == PasswordVerificationResult.Failed)
            return BadRequest(new ProblemDetails { Title = "A senha atual está incorreta." });

        if (request.CurrentPassword == request.NewPassword)
            return BadRequest(new ProblemDetails { Title = "A nova senha deve ser diferente da senha atual." });

        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<User?> GetCurrentUser(CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!ulong.TryParse(subject, out var userId)) return null;

        return await db.Users.SingleOrDefaultAsync(
            candidate => candidate.Id == userId && candidate.Status == UserStatus.Active,
            cancellationToken);
    }

    private static ProfileResponse ToResponse(User user) => new(
        user.Id,
        user.Name,
        user.Email,
        user.AvatarUrl,
        user.BirthDate,
        user.LastLoginAt,
        user.CreatedAt);
}
