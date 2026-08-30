using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MyLifeApi.Data.Converters;

public class SnakeCaseEnumConverter<TEnum>()
    : ValueConverter<TEnum, string>(
        value => ToSnakeCase(value),
        value => FromSnakeCase(value))
    where TEnum : struct, Enum
{
    private static string ToSnakeCase(TEnum value)
    {
        var name = value.ToString();
        var characters = new List<char>(name.Length + 5);

        for (var index = 0; index < name.Length; index++)
        {
            var character = name[index];

            if (index > 0 && char.IsUpper(character))
            {
                characters.Add('_');
            }

            characters.Add(char.ToLowerInvariant(character));
        }

        return new string([.. characters]);
    }

    private static TEnum FromSnakeCase(string value)
    {
        var enumName = string.Concat(
            value.Split('_', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..]));

        return Enum.Parse<TEnum>(enumName, ignoreCase: true);
    }
}
