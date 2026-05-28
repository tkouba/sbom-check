using System.Text.RegularExpressions;
using NuGet.Versioning;

namespace SbomCheck.Policy;

class IgnoreRule
{
    readonly string _namePattern;
    readonly VersionRange? _range;

    IgnoreRule(string namePattern, VersionRange? range)
    {
        _namePattern = namePattern;
        _range       = range;
    }

    public bool Matches(string componentName, string? componentVersion)
    {
        if (!MatchesName(componentName))
            return false;

        if (_range is null)
            return true; // name-only or wildcard — no version constraint

        // Inverted fail-safe: when version is unknown, do NOT silently ignore —
        // let it surface as a potential violation instead.
        if (string.IsNullOrEmpty(componentVersion))
            return false;

        if (!NuGetVersion.TryParse(componentVersion, out var version))
            return false;

        return _range.Satisfies(version);
    }

    bool MatchesName(string name)
    {
        if (!_namePattern.Contains('*'))
            return _namePattern.Equals(name, StringComparison.OrdinalIgnoreCase);

        var regexPattern = "^" + Regex.Escape(_namePattern).Replace(@"\*", ".*") + "$";
        return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase);
    }

    public static IgnoreRule Parse(string ruleText)
    {
        var atIndex = ruleText.IndexOf('@');
        if (atIndex < 0)
            return new IgnoreRule(ruleText, null);

        var name        = ruleText[..atIndex];
        var versionSpec = ruleText[(atIndex + 1)..];

        bool isRangeNotation = versionSpec.Length > 0 && versionSpec[0] is '(' or '[';

        // Normalize math notation: (-,x] → (,x]
        var normalizedSpec = isRangeNotation && versionSpec.StartsWith("(-,")
            ? "(," + versionSpec[3..]
            : versionSpec;

        var rangeText = isRangeNotation ? normalizedSpec : $"[{normalizedSpec}]";
        VersionRange.TryParse(rangeText, out var range);

        return new IgnoreRule(name, range);
    }
}
