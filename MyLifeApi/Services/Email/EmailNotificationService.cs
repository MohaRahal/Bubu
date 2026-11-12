using System.Globalization;
using Microsoft.Extensions.Options;
using MyLifeApi.Models.Enums;
using MyLifeApi.Options;

namespace MyLifeApi.Services.Email;

public sealed class EmailNotificationService(
    IEmailSender emailSender,
    IEmailTemplateRenderer templateRenderer,
    IOptions<BrevoOptions> options,
    ILogger<EmailNotificationService> logger) : IEmailNotificationService
{
    private static readonly CultureInfo BrazilianCulture =
        CultureInfo.GetCultureInfo("pt-BR");
    private readonly string _assetsBaseUrl = options.Value.AssetsBaseUrl.TrimEnd('/');

    public async Task NotifyFinancialMovementAsync(
        FinancialMovementNotification notification,
        CancellationToken cancellationToken = default)
    {
        var isIncome = notification.TransactionType == TransactionType.Income;
        var movementLabel = isIncome ? "Entrada" : "Saída";
        var templateName = isIncome ? "wallet-income" : "wallet-expense";
        var subject = isIncome
            ? "Dinheiro adicionado à sua carteira"
            : "Dinheiro retirado da sua carteira";

        var variables = new Dictionary<string, string>
        {
            ["UserName"] = notification.UserName,
            ["MovementLabel"] = movementLabel,
            ["AccountName"] = notification.AccountName,
            ["Description"] = notification.Description,
            ["Amount"] = notification.Amount.ToString("C", BrazilianCulture),
            ["TransactionDate"] = notification.TransactionDate.ToString("dd/MM/yyyy", BrazilianCulture),
            ["PreviewText"] = isIncome
                ? $"Uma entrada de {notification.Amount.ToString("C", BrazilianCulture)} foi adicionada à sua carteira."
                : $"Uma saída de {notification.Amount.ToString("C", BrazilianCulture)} foi registrada na sua carteira.",
            ["BrandIconUrl"] = AssetUrl("avatars/bubu-classico.png"),
            ["MascotUrl"] = AssetUrl(isIncome
                ? "email-assets/bubu-guardando-dinheiro.png"
                : "email-assets/bubu-tirando-dinheiro.png"),
            ["CurrentYear"] = DateTime.UtcNow.Year.ToString(BrazilianCulture)
        };

        await TrySendAsync(
            templateName,
            new EmailRecipient(notification.UserEmail, notification.UserName),
            subject,
            variables,
            ["wallet", isIncome ? "income" : "expense"],
            cancellationToken);
    }

    public async Task NotifyPasswordChangedAsync(
        string userName,
        string userEmail,
        DateTime changedAtUtc,
        CancellationToken cancellationToken = default)
    {
        const string templateName = "password-changed";
        var changedAt = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(changedAtUtc, DateTimeKind.Utc),
            ResolveSaoPauloTimeZone());
        var variables = new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["ChangedAt"] = changedAt.ToString("dd/MM/yyyy 'às' HH:mm", BrazilianCulture),
            ["PreviewText"] = "A senha da sua conta Bubu foi alterada.",
            ["BrandIconUrl"] = AssetUrl("avatars/bubu-classico.png"),
            ["MascotUrl"] = AssetUrl("avatars/bubu-classico.png"),
            ["CurrentYear"] = changedAt.Year.ToString(BrazilianCulture)
        };

        await TrySendAsync(
            templateName,
            new EmailRecipient(userEmail, userName),
            "Sua senha foi alterada",
            variables,
            ["security", "password-changed"],
            cancellationToken);
    }

    public async Task NotifyPasswordResetCodeAsync(
        string userName,
        string userEmail,
        string code,
        int lifetimeMinutes,
        CancellationToken cancellationToken = default)
    {
        const string templateName = "password-reset-code";
        var variables = new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Code"] = code,
            ["LifetimeMinutes"] = lifetimeMinutes.ToString(BrazilianCulture),
            ["PreviewText"] = $"Seu código de recuperação Bubu é {code}.",
            ["BrandIconUrl"] = AssetUrl("avatars/bubu-classico.png"),
            ["MascotUrl"] = AssetUrl("avatars/bubu-classico.png"),
            ["CurrentYear"] = DateTime.UtcNow.Year.ToString(BrazilianCulture)
        };

        await TrySendAsync(
            templateName,
            new EmailRecipient(userEmail, userName),
            $"{code} é o seu código de recuperação",
            variables,
            ["security", "password-reset"],
            cancellationToken);
    }

    private async Task TrySendAsync(
        string templateName,
        EmailRecipient recipient,
        string subject,
        IReadOnlyDictionary<string, string> variables,
        IReadOnlyList<string> tags,
        CancellationToken cancellationToken)
    {
        try
        {
            var htmlContent = await templateRenderer.RenderAsync(
                templateName,
                variables,
                cancellationToken);
            var message = new EmailMessage(recipient, subject, htmlContent, tags);
            await emailSender.SendAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Não foi possível enviar a notificação {NotificationType} para {Recipient}.",
                templateName,
                recipient.Email);
        }
    }

    private static TimeZoneInfo ResolveSaoPauloTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }

    private string AssetUrl(string relativePath) =>
        $"{_assetsBaseUrl}/{relativePath}";
}
