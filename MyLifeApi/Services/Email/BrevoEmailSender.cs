using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MyLifeApi.Options;

namespace MyLifeApi.Services.Email;

public sealed class BrevoEmailSender(
    HttpClient httpClient,
    IOptions<BrevoOptions> options,
    ILogger<BrevoEmailSender> logger) : IEmailSender
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    private readonly BrevoOptions _options = options.Value;

    public async Task SendAsync(
        EmailMessage message,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            logger.LogDebug(
                "Envio de e-mail desabilitado. Assunto: {Subject}",
                message.Subject);
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "smtp/email");
        request.Headers.Add("api-key", _options.ApiKey);
        request.Headers.Accept.ParseAdd("application/json");
        request.Content = JsonContent.Create(new
        {
            sender = new
            {
                email = _options.SenderEmail,
                name = _options.SenderName
            },
            to = new[]
            {
                new
                {
                    email = message.Recipient.Email,
                    name = message.Recipient.Name
                }
            },
            subject = message.Subject,
            htmlContent = message.HtmlContent,
            tags = message.Tags,
            attachment = message.Attachments?.Select(attachment => new
            {
                name = attachment.Name,
                content = Convert.ToBase64String(attachment.Content)
            })
        }, options: SerializerOptions);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return;

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogError(
            "A API da Brevo recusou o e-mail com status {StatusCode}: {ResponseBody}",
            (int)response.StatusCode,
            responseBody);

        throw new HttpRequestException(
            $"A API da Brevo retornou o status {(int)response.StatusCode}.",
            null,
            response.StatusCode);
    }
}
