using System.Globalization;
using Microsoft.Extensions.Options;
using MyLifeApi.Models;
using MyLifeApi.Options;

namespace MyLifeApi.Services.Email;

public sealed class FinancialReportEmailService(
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IOptions<BrevoOptions> options)
{
    private static readonly CultureInfo BrazilianCulture =
        CultureInfo.GetCultureInfo("pt-BR");
    private readonly BrevoOptions _options = options.Value;
    private readonly string _assetsBaseUrl = options.Value.AssetsBaseUrl.TrimEnd('/');

    public bool IsEnabled => _options.Enabled;

    public async Task SendAsync(
        User user,
        DateOnly from,
        DateOnly to,
        string accountName,
        byte[] pdfContent,
        CancellationToken cancellationToken = default)
    {
        var variables = new Dictionary<string, string>
        {
            ["UserName"] = user.Name,
            ["AccountName"] = accountName,
            ["Period"] = $"{from:dd/MM/yyyy} a {to:dd/MM/yyyy}",
            ["PreviewText"] = "Seu relatório financeiro Bubu está pronto.",
            ["BrandIconUrl"] = $"{_assetsBaseUrl}/avatars/bubu-classico.png",
            ["MascotUrl"] = $"{_assetsBaseUrl}/email-assets/bubu-guardando-dinheiro.png",
            ["CurrentYear"] = DateTime.UtcNow.Year.ToString(BrazilianCulture)
        };
        var htmlContent = await templateRenderer.RenderAsync(
            "financial-report",
            variables,
            cancellationToken);
        var fileName = $"relatorio-bubu-{from:yyyy-MM-dd}-{to:yyyy-MM-dd}.pdf";
        var message = new EmailMessage(
            new EmailRecipient(user.Email, user.Name),
            $"Seu relatório financeiro de {from:dd/MM} a {to:dd/MM}",
            htmlContent,
            ["finance", "report"],
            [new EmailAttachment(fileName, pdfContent, "application/pdf")]);

        await emailSender.SendAsync(message, cancellationToken);
    }
}
