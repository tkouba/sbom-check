using SbomCheck.Policy;

namespace SbomCheck.Tests;

public class ComponentRuleTests
{
    [Fact]
    public void Parse_NameOnly_NullRange()
    {
        var rule = ComponentRule.Parse("log4net");

        Assert.Equal("log4net", rule.Name);
        Assert.Null(rule.Range);
        Assert.Equal("log4net", rule.Display);
    }

    [Fact]
    public void Parse_ExactVersion_SingletonRange()
    {
        var rule = ComponentRule.Parse("log4net@2.0.1");

        Assert.Equal("log4net", rule.Name);
        Assert.NotNull(rule.Range);
    }

    [Fact]
    public void Parse_SemverRangeInclusiveUpper_ParsedCorrectly()
    {
        var rule = ComponentRule.Parse("log4net@(,2.0.1]");

        Assert.NotNull(rule.Range);
        Assert.True(rule.Matches("log4net", "1.2.10"));
        Assert.True(rule.Matches("log4net", "2.0.1"));
        Assert.False(rule.Matches("log4net", "2.0.2"));
    }

    [Fact]
    public void Parse_MathNotationRange_NormalizedToNuGetFormat()
    {
        var ruleA = ComponentRule.Parse("log4net@(-,2.0.1]");
        var ruleB = ComponentRule.Parse("log4net@(,2.0.1]");

        // Both notations should produce the same match behaviour
        Assert.True(ruleA.Matches("log4net", "1.2.10"));
        Assert.True(ruleA.Matches("log4net", "2.0.1"));
        Assert.False(ruleA.Matches("log4net", "2.0.2"));

        Assert.Equal(ruleA.Matches("log4net", "1.2.10"), ruleB.Matches("log4net", "1.2.10"));
        Assert.Equal(ruleA.Matches("log4net", "2.0.2"), ruleB.Matches("log4net", "2.0.2"));
    }

    [Fact]
    public void Matches_NameOnly_AllVersionsMatch()
    {
        var rule = ComponentRule.Parse("log4net");

        Assert.True(rule.Matches("log4net", "1.0.0"));
        Assert.True(rule.Matches("log4net", "99.99.99"));
        Assert.True(rule.Matches("log4net", null));
        Assert.True(rule.Matches("log4net", ""));
    }

    [Fact]
    public void Matches_ExactVersion_OnlyMatchesThat()
    {
        var rule = ComponentRule.Parse("log4net@2.0.1");

        Assert.True(rule.Matches("log4net", "2.0.1"));
        Assert.False(rule.Matches("log4net", "2.0.0"));
        Assert.False(rule.Matches("log4net", "2.0.2"));
    }

    [Fact]
    public void Matches_MissingVersion_FailSafeViolation()
    {
        var rule = ComponentRule.Parse("log4net@(,2.0.1]");

        Assert.True(rule.Matches("log4net", null));
        Assert.True(rule.Matches("log4net", ""));
    }

    [Fact]
    public void Matches_UnparseableVersion_FailSafeViolation()
    {
        var rule = ComponentRule.Parse("log4net@(,2.0.1]");

        Assert.True(rule.Matches("log4net", "not-a-version"));
    }

    [Fact]
    public void Matches_DifferentName_NoMatch()
    {
        var rule = ComponentRule.Parse("log4net");

        Assert.False(rule.Matches("Serilog", "3.0.0"));
    }

    [Fact]
    public void Matches_NameMatchIsCaseInsensitive()
    {
        var rule = ComponentRule.Parse("log4net");

        Assert.True(rule.Matches("LOG4NET", "2.0.0"));
        Assert.True(rule.Matches("Log4Net", "1.0.0"));
    }
}
