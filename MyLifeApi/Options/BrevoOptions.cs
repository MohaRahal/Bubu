using System.ComponentModel.DataAnnotations;

namespace MyLifeApi.Options;

public sealed class BrevoOptions
{
    public const string SectionName = "Brevo";

    public bool Enabled { get; set; }

    public string ApiUrl { get; set; } = "https://api.brevo.com/v3/";

    public string ApiKey { get; set; } = string.Empty;

    public string AssetsBaseUrl { get; set; } = string.Empty;

    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "Bubu";
}
