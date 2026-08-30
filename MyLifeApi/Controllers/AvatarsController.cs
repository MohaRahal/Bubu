using Microsoft.AspNetCore.Mvc;
using MyLifeApi.DTOs.Avatars;
using MyLifeApi.Services;

namespace MyLifeApi.Controllers;

[ApiController]
[Route("api/avatars")]
public sealed class AvatarsController(AvatarCatalogService avatarCatalog) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<AvatarOptionResponse>> GetAll() =>
        Ok(avatarCatalog.GetAll());
}
