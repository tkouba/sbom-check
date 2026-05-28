using SbomCheck.Sbom.Models;

namespace SbomCheck.Tests;

public class ComponentTests
{
    [Fact]
    public void GetLicenseIds_NullLicenses_ReturnsUnknown()
    {
        var c = new Component("Foo", "1.0", null);
        Assert.Equal(["UNKNOWN"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_EmptyLicensesList_ReturnsUnknown()
    {
        var c = new Component("Foo", "1.0", []);
        Assert.Equal(["UNKNOWN"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_SpdxId_ReturnsSpdxId()
    {
        var c = ComponentWith(id: "MIT", name: null);
        Assert.Equal(["MIT"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_EmptySpdxId_FallsBackToName()
    {
        var c = ComponentWith(id: "", name: "Custom-1.0");
        Assert.Equal(["Custom-1.0"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_WhitespaceSpdxId_FallsBackToName()
    {
        var c = ComponentWith(id: "   ", name: "Proprietary");
        Assert.Equal(["Proprietary"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_NullSpdxId_FallsBackToName()
    {
        var c = ComponentWith(id: null, name: "Proprietary");
        Assert.Equal(["Proprietary"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_EmptyIdAndName_ReturnsUnknown()
    {
        var c = ComponentWith(id: "", name: "");
        Assert.Equal(["UNKNOWN"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_NullLicenseObject_ReturnsUnknown()
    {
        var c = new Component("Foo", "1.0", [new LicenseChoice(null)]);
        Assert.Equal(["UNKNOWN"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_DuplicateLicenses_Deduplicates()
    {
        var c = new Component("Foo", "1.0",
        [
            new LicenseChoice(new LicenseInfo("MIT", null)),
            new LicenseChoice(new LicenseInfo("MIT", null))
        ]);
        Assert.Equal(["MIT"], c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_DuplicateDifferentCase_Deduplicates()
    {
        var c = new Component("Foo", "1.0",
        [
            new LicenseChoice(new LicenseInfo("mit", null)),
            new LicenseChoice(new LicenseInfo("MIT", null))
        ]);
        Assert.Single(c.GetLicenseIds().ToArray());
    }

    [Fact]
    public void GetLicenseIds_MultipleDifferentLicenses_ReturnsAll()
    {
        var c = new Component("Foo", "1.0",
        [
            new LicenseChoice(new LicenseInfo("MIT", null)),
            new LicenseChoice(new LicenseInfo("Apache-2.0", null))
        ]);
        var ids = c.GetLicenseIds().ToArray();
        Assert.Equal(2, ids.Length);
        Assert.Contains("MIT", ids);
        Assert.Contains("Apache-2.0", ids);
    }

    static Component ComponentWith(string? id, string? name) =>
        new("Foo", "1.0", [new LicenseChoice(new LicenseInfo(id, name))]);
}
