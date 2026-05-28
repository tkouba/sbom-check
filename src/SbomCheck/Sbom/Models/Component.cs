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

        foreach (var choice in Licenses)
        {
            yield return choice.License?.Id
                ?? choice.License?.Name
                ?? "UNKNOWN";
        }
    }
}
