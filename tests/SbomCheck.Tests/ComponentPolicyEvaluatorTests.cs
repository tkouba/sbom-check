using SbomCheck.Policy;
using SbomCheck.Sbom.Models;

namespace SbomCheck.Tests;

public class ComponentPolicyEvaluatorTests
{
    [Fact]
    public void Evaluate_NoRules_ReturnsEmpty()
    {
        var bom = Bom([("log4net", "2.0.0")]);
        var result = ComponentPolicyEvaluator.Evaluate(bom, []);
        Assert.Empty(result);
    }

    [Fact]
    public void Evaluate_NameOnly_MatchesAllVersions()
    {
        var bom = Bom([("log4net", "1.2.10"), ("log4net", "2.0.15"), ("Serilog", "3.0.0")]);
        var result = ComponentPolicyEvaluator.Evaluate(bom, ["log4net"]);

        Assert.Single(result);
        Assert.Equal(2, result[0].Components.Count);
    }

    [Fact]
    public void Evaluate_ExactVersion_MatchesOnlyThat()
    {
        var bom = Bom([("log4net", "2.0.0"), ("log4net", "2.0.15")]);
        var result = ComponentPolicyEvaluator.Evaluate(bom, ["log4net@2.0.0"]);

        Assert.Single(result);
        Assert.Single(result[0].Components);
        Assert.Equal("2.0.0", result[0].Components[0].Version);
    }

    [Fact]
    public void Evaluate_VersionRange_MatchesInRange()
    {
        var bom = Bom([("log4net", "1.2.10"), ("log4net", "2.0.1"), ("log4net", "2.0.15")]);
        var result = ComponentPolicyEvaluator.Evaluate(bom, ["log4net@(-,2.0.1]"]);

        Assert.Single(result);
        var matched = result[0].Components.Select(c => c.Version).ToHashSet();
        Assert.Contains("1.2.10", matched);
        Assert.Contains("2.0.1", matched);
        Assert.DoesNotContain("2.0.15", matched);
    }

    [Fact]
    public void Evaluate_ComponentNotInBom_NoViolation()
    {
        var bom = Bom([("Serilog", "3.0.0")]);
        var result = ComponentPolicyEvaluator.Evaluate(bom, ["log4net"]);
        Assert.Empty(result);
    }

    [Fact]
    public void Evaluate_NameMatchIsCaseInsensitive()
    {
        var bom = Bom([("LOG4NET", "2.0.0")]);
        var result = ComponentPolicyEvaluator.Evaluate(bom, ["log4net"]);
        Assert.Single(result);
    }

    [Fact]
    public void Evaluate_MultipleRules_EachProducesOwnViolation()
    {
        var bom = Bom([("log4net", "1.2.10"), ("log4net", "2.0.15")]);
        var result = ComponentPolicyEvaluator.Evaluate(bom, ["log4net@1.2.10", "log4net@2.0.15"]);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Evaluate_MissingVersion_FailSafe()
    {
        var bom = new BomDocument { Components = [new Component("log4net", null, null)] };
        var result = ComponentPolicyEvaluator.Evaluate(bom, ["log4net@(,2.0.1]"]);

        Assert.Single(result);
    }

    static BomDocument Bom(IEnumerable<(string name, string version)> entries) =>
        new() { Components = entries.Select(e => new Component(e.name, e.version, null)).ToList() };
}
