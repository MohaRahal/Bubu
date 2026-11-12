using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    PasswordResetService passwordResetService,
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
    [EnableRateLimiting("password-reset-request")]
    [HttpPost("password-reset/request")]
    public async Task<ActionResult<PasswordResetRequestedResponse>> RequestPasswordReset(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await passwordResetService.RequestAsync(
            request.Email,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return Accepted(new PasswordResetRequestedResponse(
            "Se o e-mail estiver cadastrado, enviaremos um código de recuperação.",
            passwordResetService.CodeLifetimeMinutes));
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-reset-confirm")]
    [HttpPost("password-reset/validate")]
    public async Task<ActionResult<PasswordResetCodeValidatedResponse>> ValidatePasswordResetCode(
        ValidatePasswordResetCodeRequest request,
        CancellationToken cancellationToken)
    {
        var resetToken = await passwordResetService.ValidateCodeAsync(
            request.Email,
            request.Code,
            cancellationToken);

        return resetToken is not null
            ? Ok(new PasswordResetCodeValidatedResponse(resetToken))
            : BadRequest(CreateInvalidResetProblem());
    }

    [AllowAnonymous]
    [EnableRateLimiting("password-reset-confirm")]
    [HttpPost("password-reset/confirm")]
    public async Task<IActionResult> ConfirmPasswordReset(
        CompletePasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var succeeded = await passwordResetService.CompleteAsync(
            request.Email,
            request.ResetToken,
            request.NewPassword,
            cancellationToken);

        return succeeded
            ? NoContent()
            : BadRequest(CreateInvalidResetProblem());
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

    private static ProblemDetails CreateInvalidResetProblem() => new()
    {
        Title = "Código inválido ou expirado.",
        Detail = "Solicite um novo código e tente novamente."
    };

    private static AuthenticatedUserResponse ToResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.AvatarUrl);
}
