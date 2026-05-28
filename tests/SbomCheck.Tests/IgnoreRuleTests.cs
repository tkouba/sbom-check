using SbomCheck.Policy;

namespace SbomCheck.Tests;

public class IgnoreRuleTests
{
    [Fact]
    public void Parse_NameOnly_MatchesAllVersions()
    {
        var rule = IgnoreRule.Parse("log4net");

        Assert.True(rule.Matches("log4net", "2.0.0"));
        Assert.True(rule.Matches("log4net", null));
        Assert.True(rule.Matches("log4net", ""));
    }

    [Fact]
    public void Matches_WildcardSuffix_MatchesPattern()
    {
        var rule = IgnoreRule.Parse("Microsoft*");

        Assert.True(rule.Matches("Microsoft.Extensions.Logging", "8.0.0"));
        Assert.True(rule.Matches("Microsoft.AspNetCore", "8.0.0"));
        Assert.False(rule.Matches("Newtonsoft.Json", "13.0.3"));
    }

    [Fact]
    public void Matches_ExactVersion_MatchesOnlyThat()
    {
        var rule = IgnoreRule.Parse("log4net@2.0.1");

        Assert.True(rule.Matches("log4net", "2.0.1"));
        Assert.False(rule.Matches("log4net", "2.0.0"));
        Assert.False(rule.Matches("log4net", "2.0.15"));
    }

    [Fact]
    public void Matches_VersionRange_MatchesInRange()
    {
        var rule = IgnoreRule.Parse("log4net@(-,2.0.1]");

        Assert.True(rule.Matches("log4net", "1.2.10"));
        Assert.True(rule.Matches("log4net", "2.0.1"));
        Assert.False(rule.Matches("log4net", "2.0.15"));
    }

    [Fact]
    public void Matches_MissingVersion_NotIgnored()
    {
        var rule = IgnoreRule.Parse("log4net@(,2.0.1]");

        Assert.False(rule.Matches("log4net", null));
        Assert.False(rule.Matches("log4net", ""));
    }

    [Fact]
    public void Matches_UnparseableVersion_NotIgnored()
    {
        var rule = IgnoreRule.Parse("log4net@(,2.0.1]");

        Assert.False(rule.Matches("log4net", "not-a-version"));
    }

    [Fact]
    public void Matches_CaseInsensitiveName()
    {
        var rule = IgnoreRule.Parse("log4net");

        Assert.True(rule.Matches("LOG4NET", "2.0.0"));
        Assert.True(rule.Matches("Log4Net", "1.0.0"));
    }

    [Fact]
    public void Matches_WildcardWithVersionRange_BothApply()
    {
        var rule = IgnoreRule.Parse("log4net@(-,2.0.1]");

        Assert.True(rule.Matches("log4net", "1.2.10"));
        Assert.False(rule.Matches("log4net", "2.0.15"));
        // name mismatch still excludes
        Assert.False(rule.Matches("Serilog", "1.0.0"));
    }
}
