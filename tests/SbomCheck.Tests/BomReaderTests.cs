using SbomCheck.Sbom;

namespace SbomCheck.Tests;

public class BomReaderTests
{
    [Fact]
    public void TryRead_FileNotFound_ReturnsNullWithError()
    {
        var result = BomReader.TryRead("nonexistent_file_xyz.json", out var error);

        Assert.Null(result);
        Assert.NotNull(error);
        Assert.Contains("not found", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryRead_InvalidJson_ReturnsNullWithError()
    {
        var path = TempFile("{ this is not valid json }");
        try
        {
            var result = BomReader.TryRead(path, out var error);

            Assert.Null(result);
            Assert.NotNull(error);
            Assert.Contains("Invalid JSON", error, StringComparison.OrdinalIgnoreCase);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void TryRead_EmptyFile_ReturnsNullWithError()
    {
        var path = TempFile("");
        try
        {
            var result = BomReader.TryRead(path, out var error);

            Assert.Null(result);
            Assert.NotNull(error);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void TryRead_NullJson_ReturnsNullWithError()
    {
        var path = TempFile("null");
        try
        {
            var result = BomReader.TryRead(path, out var error);

            Assert.Null(result);
            Assert.NotNull(error);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void TryRead_ValidBom_ReturnsDocument()
    {
        var path = TempFile("""
            {
              "bomFormat": "CycloneDX",
              "components": [
                { "name": "Foo", "version": "1.0.0", "licenses": [{ "license": { "id": "MIT" } }] }
              ]
            }
            """);
        try
        {
            var result = BomReader.TryRead(path, out var error);

            Assert.NotNull(result);
            Assert.Null(error);
            Assert.Single(result.Components);
            Assert.Equal("Foo", result.Components[0].Name);
            Assert.Equal("1.0.0", result.Components[0].Version);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void TryRead_EmptyComponents_ReturnsEmptyList()
    {
        var path = TempFile("""{"bomFormat":"CycloneDX","components":[]}""");
        try
        {
            var result = BomReader.TryRead(path, out var error);

            Assert.NotNull(result);
            Assert.Null(error);
            Assert.Empty(result.Components);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void TryRead_MissingComponentsField_ReturnsEmptyListNotNull()
    {
        var path = TempFile("""{"bomFormat":"CycloneDX","specVersion":"1.4"}""");
        try
        {
            var result = BomReader.TryRead(path, out var error);

            Assert.NotNull(result);
            Assert.Null(error);
            Assert.NotNull(result.Components);
            Assert.Empty(result.Components);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void TryRead_ComponentWithNoLicensesField_Succeeds()
    {
        var path = TempFile("""
            {
              "bomFormat": "CycloneDX",
              "components": [
                { "name": "LegacyLib", "version": "2.0.0" }
              ]
            }
            """);
        try
        {
            var result = BomReader.TryRead(path, out var error);

            Assert.NotNull(result);
            Assert.Null(error);
            Assert.Equal("UNKNOWN", result.Components[0].GetLicenseIds().Single());
        }
        finally { File.Delete(path); }
    }

    static string TempFile(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        return path;
    }
}
