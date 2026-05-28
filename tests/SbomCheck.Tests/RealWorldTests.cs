using SbomCheck.Models;
using SbomCheck.Policy;
using SbomCheck.Sbom;

namespace SbomCheck.Tests;

public class RealWorldTests
{
    static readonly string RealWorldPath =
        Path.Combine(AppContext.BaseDirectory, "TestData", "realWorld.json");

    [Fact]
    public void RealWorld_ParsesAllComponents()
    {
        var bom = BomReader.TryRead(RealWorldPath, out var error);

        Assert.NotNull(bom);
        Assert.Null(error);
        Assert.Equal(166, bom.Components.Count);
    }

    [Fact]
    public void RealWorld_NoPolicy_StatusIsNone()
    {
        var bom = BomReader.TryRead(RealWorldPath, out _)!;
        var result = LicensePolicyEvaluator.Evaluate(bom, [], []);

        Assert.Equal(LicenseStatus.None, result.Status);
    }

    [Fact]
    public void RealWorld_NoPolicy_SummaryContainsExpectedLicenses()
    {
        var bom = BomReader.TryRead(RealWorldPath, out _)!;
        var result = LicensePolicyEvaluator.Evaluate(bom, [], []);

        var ids = result.LicenseDetails.Select(d => d.LicenseId).ToHashSet();
        Assert.Contains("MIT", ids);
        Assert.Contains("Apache-2.0", ids);
        Assert.Contains("BSD-2-Clause", ids);
    }

    [Fact]
    public void RealWorld_ForbidBSDLicense_DetectsViolation()
    {
        var bom = BomReader.TryRead(RealWorldPath, out _)!;
        var result = LicensePolicyEvaluator.Evaluate(bom, ["BSD-2-Clause"], []);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        var bsdDetail = result.LicenseDetails.Single(d => d.LicenseId == "BSD-2-Clause");
        Assert.Equal(ViolationReason.Forbidden, bsdDetail.ViolationReason);
        Assert.True(bsdDetail.Count > 0);
    }

    [Fact]
    public void RealWorld_AllowOnlyMIT_DetectsNonMITViolations()
    {
        var bom = BomReader.TryRead(RealWorldPath, out _)!;
        var result = LicensePolicyEvaluator.Evaluate(bom, [], ["MIT"]);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        var violations = result.LicenseDetails
            .Where(d => d.ViolationReason == ViolationReason.NotAllowed)
            .Select(d => d.LicenseId)
            .ToHashSet();
        Assert.Contains("Apache-2.0", violations);
        Assert.Contains("BSD-2-Clause", violations);
    }

    [Fact]
    public void RealWorld_AllowAllPresentLicenses_ReturnsValid()
    {
        var bom = BomReader.TryRead(RealWorldPath, out _)!;
        // Collect all license IDs that actually appear so the allowed list is exhaustive
        var result0 = LicensePolicyEvaluator.Evaluate(bom, [], []);
        var allPresent = result0.LicenseDetails.Select(d => d.LicenseId).ToArray();

        var result = LicensePolicyEvaluator.Evaluate(bom, [], allPresent);

        Assert.Equal(LicenseStatus.Valid, result.Status);
    }
}
