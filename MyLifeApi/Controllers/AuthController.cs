using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyLifeApi.Data;
using MyLifeApi.DTOs.Auth;
using MyLifeApi.Models;
using MyLifeApi.Models.Enums;
using MyLifeApi.Options;
using MyLifeApi.Services;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    MyLifeDbContext db,
    IPasswordHasher<User> passwordHasher,
    AuthTokenService tokenService,
    IOptions<JwtOptions> jwtOptions,
    IHostEnvironment environment) : ControllerBase
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticatedUserResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(
            candidate => candidate.Email == email,
            cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
            return Unauthorized(CreateInvalidCredentialsProblem());

        var verificationResult = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
            return Unauthorized(CreateInvalidCredentialsProblem());

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        var accessToken = tokenService.Create(user);
        Response.Cookies.Append(
            _jwtOptions.CookieName,
            accessToken.Value,
            CreateCookieOptions(accessToken.ExpiresAtUtc));

        return Ok(ToResponse(user));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<AuthenticatedUserResponse>> Me(
        CancellationToken cancellationToken)
    {
        var subject = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!ulong.TryParse(subject, out var userId))
            return Unauthorized();

        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.Id == userId && candidate.Status == UserStatus.Active,
            cancellationToken);

        return user is null ? Unauthorized() : Ok(ToResponse(user));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(_jwtOptions.CookieName, CreateCookieOptions());
        return NoContent();
    }

    private CookieOptions CreateCookieOptions(DateTime? expiresAtUtc = null) => new()
    {
        HttpOnly = true,
        Secure = !environment.IsDevelopment(),
        SameSite = SameSiteMode.Strict,
        Path = "/",
        Expires = expiresAtUtc
    };

    private static ProblemDetails CreateInvalidCredentialsProblem() => new()
    {
        Status = StatusCodes.Status401Unauthorized,
        Title = "E-mail ou senha inválidos."
    };

    private static AuthenticatedUserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.AvatarUrl);
}
