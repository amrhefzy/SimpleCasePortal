using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace SimpleCasePortal.Infrastructure.Security;

internal static partial class SensitiveAuditValueMasker
{
    private static readonly string[] SensitiveKeyFragments =
    [
        "password",
        "passwordHash",
        "token",
        "accessToken",
        "refreshToken",
        "apiKey",
        "secret",
        "authorization",
        "signedUrl",
        "downloadUrl",
        "connectionString"
    ];

    public static string? MaskAndFormat(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        try
        {
            var node = JsonNode.Parse(value);
            MaskJsonNode(node);
            return node?.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }
        catch (JsonException)
        {
            return MaskText(value);
        }
    }

    public static string CreateSummary(string? oldValues, string? newValues)
    {
        var source = MaskAndFormat(newValues) ?? MaskAndFormat(oldValues) ?? string.Empty;
        source = WhitespaceRegex().Replace(source, " ").Trim();
        return source.Length <= 140 ? source : $"{source[..140]}...";
    }

    private static void MaskJsonNode(JsonNode? node)
    {
        if (node is JsonObject jsonObject)
        {
            foreach (var propertyName in jsonObject.Select(property => property.Key).ToArray())
            {
                if (IsSensitiveKey(propertyName))
                {
                    jsonObject[propertyName] = "***MASKED***";
                }
                else
                {
                    MaskJsonNode(jsonObject[propertyName]);
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var child in jsonArray)
            {
                MaskJsonNode(child);
            }
        }
    }

    private static string MaskText(string value)
    {
        var maskedValue = value;
        foreach (var key in SensitiveKeyFragments)
        {
            maskedValue = Regex.Replace(
                maskedValue,
                $@"(?i)({Regex.Escape(key)}\s*[:=]\s*)(""[^""]*""|'[^']*'|[^\s,;}}]+)",
                "$1***MASKED***");
        }

        return maskedValue;
    }

    private static bool IsSensitiveKey(string key)
    {
        return SensitiveKeyFragments.Any(fragment =>
            key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}
