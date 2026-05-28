using SbomCheck.Models;
using SbomCheck.Policy;
using SbomCheck.Sbom.Models;

namespace SbomCheck.Tests;

public class LicensePolicyEvaluatorTests
{
    [Fact]
    public void Evaluate_NoPolicy_ReturnsNoneStatus()
    {
        var bom = Bom([("Foo", "MIT"), ("Bar", "Apache-2.0")]);
        var result = LicensePolicyEvaluator.Evaluate(bom, [], []);
        Assert.Equal(LicenseStatus.None, result.Status);
        Assert.All(result.LicenseDetails, d => Assert.Equal(ViolationReason.None, d.ViolationReason));
    }

    [Fact]
    public void Evaluate_ForbiddenLicense_DetectsViolation()
    {
        var bom = Bom([("Foo", "GPL-3.0"), ("Bar", "MIT")]);
        var result = LicensePolicyEvaluator.Evaluate(bom, ["GPL-3.0"], []);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        var detail = result.LicenseDetails.Single(d => d.LicenseId == "GPL-3.0");
        Assert.Equal(ViolationReason.Forbidden, detail.ViolationReason);
        Assert.Contains(detail.Components, c => c.Name == "Foo");
    }

    [Fact]
    public void Evaluate_NonForbiddenLicense_NotViolation()
    {
        var bom = Bom([("Foo", "MIT")]);
        var result = LicensePolicyEvaluator.Evaluate(bom, ["GPL-3.0"], []);

        Assert.Equal(LicenseStatus.Valid, result.Status);
        Assert.DoesNotContain(result.LicenseDetails, d => d.ViolationReason == ViolationReason.Forbidden);
    }

    [Fact]
    public void Evaluate_AllowedList_FlagsUnlisted()
    {
        var bom = Bom([("Foo", "MIT"), ("Bar", "GPL-3.0")]);
        var result = LicensePolicyEvaluator.Evaluate(bom, [], ["MIT"]);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        var violation = result.LicenseDetails.Single(d => d.ViolationReason == ViolationReason.NotAllowed);
        Assert.Equal("GPL-3.0", violation.LicenseId);
    }

    [Fact]
    public void Evaluate_AllLicensesInAllowedList_ReturnsValid()
    {
        var bom = Bom([("Foo", "MIT"), ("Bar", "Apache-2.0")]);
        var result = LicensePolicyEvaluator.Evaluate(bom, [], ["MIT", "Apache-2.0"]);

        Assert.Equal(LicenseStatus.Valid, result.Status);
        Assert.DoesNotContain(result.LicenseDetails, d => d.ViolationReason != ViolationReason.None);
    }

    [Fact]
    public void Evaluate_ForbiddenTakesPriorityOverAllowed()
    {
        // GPL-3.0 is both forbidden and in the allowed list — forbidden wins
        var bom = Bom([("Foo", "GPL-3.0")]);
        var result = LicensePolicyEvaluator.Evaluate(bom, ["GPL-3.0"], ["GPL-3.0"]);

        var detail = result.LicenseDetails.Single(d => d.LicenseId == "GPL-3.0");
        Assert.Equal(ViolationReason.Forbidden, detail.ViolationReason);
    }

    [Fact]
    public void Evaluate_EmptyBom_NoViolations()
    {
        var bom = Bom([]);
        var result = LicensePolicyEvaluator.Evaluate(bom, ["GPL-3.0"], []);

        Assert.Equal(LicenseStatus.Valid, result.Status);
        Assert.Empty(result.LicenseDetails);
    }

    [Fact]
    public void Evaluate_LicenseMatchingIsCaseInsensitive()
    {
        var bom = Bom([("Foo", "gpl-3.0")]);
        var result = LicensePolicyEvaluator.Evaluate(bom, ["GPL-3.0"], []);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
    }

    [Fact]
    public void Evaluate_UnknownLicense_CountedCorrectly()
    {
        var bom = new BomDocument { Components = [new Component("Foo", "1.0", null)] };
        var result = LicensePolicyEvaluator.Evaluate(bom, [], []);

        Assert.Equal(1, result.LicenseDetails.Single(d => d.LicenseId == "UNKNOWN").Count);
    }

    [Fact]
    public void Evaluate_MultiLicenseComponent_BothLicensesTracked()
    {
        var bom = new BomDocument
        {
            Components =
            [
                new Component("Foo", "1.0",
                [
                    new LicenseChoice(new LicenseInfo("MIT", null)),
                    new LicenseChoice(new LicenseInfo("Apache-2.0", null))
                ])
            ]
        };
        var result = LicensePolicyEvaluator.Evaluate(bom, ["Apache-2.0"], []);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        Assert.Single(result.LicenseDetails, d => d.ViolationReason == ViolationReason.Forbidden);
        var mitDetail = result.LicenseDetails.Single(d => d.LicenseId == "MIT");
        Assert.Equal(ViolationReason.None, mitDetail.ViolationReason);
    }

    static BomDocument Bom(IEnumerable<(string name, string licenseId)> entries) =>
        new()
        {
            Components = entries
                .Select(e => new Component(e.name, "1.0",
                [
                    new LicenseChoice(new LicenseInfo(e.licenseId, null))
                ]))
                .ToList()
        };
}
