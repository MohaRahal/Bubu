namespace MyLifeApi.Services.Email;

public sealed record EmailRecipient(string Email, string Name);

public sealed record EmailAttachment(
    string Name,
    byte[] Content,
    string ContentType);

public sealed record EmailMessage(
    EmailRecipient Recipient,
    string Subject,
    string HtmlContent,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyList<EmailAttachment>? Attachments = null);
