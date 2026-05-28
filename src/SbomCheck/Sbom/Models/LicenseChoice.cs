using System.Text.Json.Serialization;

namespace SbomCheck.Sbom.Models;

record LicenseChoice(
    [property: JsonPropertyName("license")] LicenseInfo? License
);

record LicenseInfo(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name
);
