using System.Globalization;
using MyLifeApi.DTOs.Avatars;

namespace MyLifeApi.Services;

public sealed class AvatarCatalogService(IWebHostEnvironment environment)
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp" };

    private readonly string _avatarsDirectory = Path.Combine(
        environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
        "avatars");

    public IReadOnlyList<AvatarOptionResponse> GetAll()
    {
        if (!Directory.Exists(_avatarsDirectory)) return [];

        return Directory.EnumerateFiles(_avatarsDirectory)
            .Where(file => AllowedExtensions.Contains(Path.GetExtension(file)))
            .Select(Path.GetFileName)
            .Where(fileName => fileName is not null)
            .Select(fileName => new AvatarOptionResponse(
                fileName!,
                CreateDisplayName(fileName!),
                CreatePublicPath(fileName!)))
            .OrderBy(avatar => avatar.Name)
            .ToList();
    }

    public bool TryResolve(string? avatarKey, out string? publicPath)
    {
        publicPath = null;
        if (string.IsNullOrWhiteSpace(avatarKey)) return true;

        var fileName = Path.GetFileName(avatarKey);
        if (!string.Equals(fileName, avatarKey, StringComparison.Ordinal) ||
            !AllowedExtensions.Contains(Path.GetExtension(fileName)))
        {
            return false;
        }

        var fullPath = Path.Combine(_avatarsDirectory, fileName);
        if (!File.Exists(fullPath)) return false;

        publicPath = CreatePublicPath(fileName);
        return true;
    }

    private static string CreatePublicPath(string fileName) =>
        $"/avatars/{Uri.EscapeDataString(fileName)}";

    private static string CreateDisplayName(string fileName)
    {
        var words = Path.GetFileNameWithoutExtension(fileName)
            .Replace('-', ' ')
            .Replace('_', ' ');

        return CultureInfo.GetCultureInfo("pt-BR").TextInfo.ToTitleCase(words);
    }
}
