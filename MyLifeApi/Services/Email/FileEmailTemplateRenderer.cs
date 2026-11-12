using System.Text.Encodings.Web;
using System.Text.RegularExpressions;

namespace MyLifeApi.Services.Email;

public sealed class FileEmailTemplateRenderer(IHostEnvironment environment)
    : IEmailTemplateRenderer
{
    private static readonly Regex PlaceholderPattern = new(
        "{{\\s*(?<name>[A-Za-z][A-Za-z0-9]*)\\s*}}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly string _templatesPath = Path.Combine(
        environment.ContentRootPath,
        "EmailTemplates");

    public async Task<string> RenderAsync(
        string templateName,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken cancellationToken = default)
    {
        if (Path.GetFileName(templateName) != templateName)
            throw new ArgumentException("Nome de template inválido.", nameof(templateName));

        var layoutPath = Path.Combine(_templatesPath, "_layout.html");
        var templatePath = Path.Combine(_templatesPath, $"{templateName}.html");

        var layout = await File.ReadAllTextAsync(layoutPath, cancellationToken);
        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        var renderedBody = ReplaceVariables(template, variables);
        var renderedLayout = layout.Replace("{{Content}}", renderedBody, StringComparison.Ordinal);

        return ReplaceVariables(renderedLayout, variables);
    }

    private static string ReplaceVariables(
        string content,
        IReadOnlyDictionary<string, string> variables) =>
        PlaceholderPattern.Replace(content, match =>
        {
            var name = match.Groups["name"].Value;
            if (!variables.TryGetValue(name, out var value))
                throw new InvalidOperationException(
                    $"A variável '{name}' não foi informada para o template de e-mail.");

            return HtmlEncoder.Default.Encode(value);
        });
}
