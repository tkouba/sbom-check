using System.Text.Json.Serialization;

namespace SbomCheck.Sbom.Models;

record BomDocument(
    [property: JsonPropertyName("components")] List<Component> Components
);
