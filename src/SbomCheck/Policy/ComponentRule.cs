using NuGet.Versioning;

namespace SbomCheck.Policy;

record ComponentRule(string Name, VersionRange? Range, string Display)
{
    public bool Matches(string componentName, string? componentVersion)
    {
        if (!componentName.Equals(Name, StringComparison.OrdinalIgnoreCase))
            return false;

        if (Range is null)
            return true; // name-only rule — all versions match

        if (string.IsNullOrEmpty(componentVersion))
            return true; // fail-safe: missing version in BOM

        if (!NuGetVersion.TryParse(componentVersion, out var version))
            return true; // fail-safe: unparseable version in BOM

        return Range.Satisfies(version);
    }

    public static ComponentRule Parse(string ruleText)
    {
        var atIndex = ruleText.IndexOf('@');
        if (atIndex < 0)
            return new ComponentRule(ruleText, null, ruleText);

        var name        = ruleText[..atIndex];
        var versionSpec = ruleText[(atIndex + 1)..];

        bool isRangeNotation = versionSpec.Length > 0 && versionSpec[0] is '(' or '[';

        // Normalize math unbounded-below notation: (-,x] → (,x]
        // NuGet.Versioning uses (,x] for unbounded lower, not (-,x]
        var normalizedSpec = isRangeNotation && versionSpec.StartsWith("(-,")
            ? "(," + versionSpec[3..]
            : versionSpec;

        var rangeText = isRangeNotation ? normalizedSpec : $"[{normalizedSpec}]";

        VersionRange.TryParse(rangeText, out var range);

        // Fall back to name-only (match all) when spec can't be parsed
        return new ComponentRule(name, range, ruleText);
    }
}
