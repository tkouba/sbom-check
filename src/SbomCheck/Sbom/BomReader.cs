using System.Text.Json;
using SbomCheck.Sbom.Models;

namespace SbomCheck.Sbom;

static class BomReader
{
    static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static BomDocument? TryRead(string path, out string? error)
    {
        if (!File.Exists(path))
        {
            error = $"File not found: {path}";
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            var doc = JsonSerializer.Deserialize<BomDocument>(json, Options);
            if (doc is null)
            {
                error = "Failed to parse bom.json: document is null.";
                return null;
            }
            error = null;
            return doc;
        }
        catch (JsonException ex)
        {
            error = $"Invalid JSON: {ex.Message}";
            return null;
        }
    }
}
