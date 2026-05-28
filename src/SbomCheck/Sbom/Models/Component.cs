using System.Text.Json.Serialization;

namespace SbomCheck.Sbom.Models;

record Component(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("version")] string? Version,
    [property: JsonPropertyName("licenses")] List<LicenseChoice>? Licenses
)
{
    public IEnumerable<string> GetLicenseIds()
    {
        if (Licenses is null || Licenses.Count == 0)
        {
            yield return "UNKNOWN";
            yield break;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var choice in Licenses)
        {
            // Fallback chain: SPDX id → name → UNKNOWN
            // Empty/whitespace values are treated as absent and skipped
            var id = NonEmpty(choice.License?.Id)
                  ?? NonEmpty(choice.License?.Name)
                  ?? "UNKNOWN";

            if (seen.Add(id))
                yield return id;
        }
    }

    static string? NonEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
