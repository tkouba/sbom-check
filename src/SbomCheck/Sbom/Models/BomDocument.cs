using System.Text.Json.Serialization;

namespace SbomCheck.Sbom.Models;

record BomDocument
{
    [JsonPropertyName("components")]
    public List<Component> Components { get; init; } = [];
}
