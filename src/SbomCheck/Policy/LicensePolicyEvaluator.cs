using SbomCheck.Models;
using SbomCheck.Sbom.Models;

namespace SbomCheck.Policy;

static class LicensePolicyEvaluator
{
    public static LicensesResult Evaluate(
        BomDocument bom,
        IEnumerable<string> forbiddenLicenses,
        IEnumerable<string> allowedLicenses)
    {
        var forbidden = new HashSet<string>(forbiddenLicenses, StringComparer.OrdinalIgnoreCase);
        var allowed   = new HashSet<string>(allowedLicenses,   StringComparer.OrdinalIgnoreCase);
        bool hasPolicy = forbidden.Count > 0 || allowed.Count > 0;

        // licenseId → components using it
        var licenseMap = new Dictionary<string, List<LicenseComponent>>(StringComparer.OrdinalIgnoreCase);

        foreach (var component in bom.Components)
        {
            var entry = new LicenseComponent(component.Name, component.Version ?? "");
            foreach (var licenseId in component.GetLicenseIds())
            {
                if (!licenseMap.TryGetValue(licenseId, out var list))
                    licenseMap[licenseId] = list = [];
                list.Add(entry);
            }
        }

        var details = licenseMap
            .Select(kv =>
            {
                var reason = ResolveViolationReason(kv.Key, forbidden, allowed);
                return new LicenseDetail
                {
                    LicenseId       = kv.Key,
                    Count           = kv.Value.Count,
                    Status          = hasPolicy
                                        ? (reason != ViolationReason.None ? LicenseStatus.Invalid : LicenseStatus.Valid)
                                        : LicenseStatus.None,
                    ViolationReason = reason,
                    Components      = reason != ViolationReason.None ? kv.Value.ToArray() : []
                };
            })
            .ToList();

        var overallStatus = hasPolicy
            ? (details.Any(d => d.Status == LicenseStatus.Invalid) ? LicenseStatus.Invalid : LicenseStatus.Valid)
            : LicenseStatus.None;

        return new LicensesResult
        {
            Status          = overallStatus,
            TotalComponents = bom.Components.Count,
            LicenseDetails  = details
        };
    }

    static ViolationReason ResolveViolationReason(
        string licenseId,
        HashSet<string> forbidden,
        HashSet<string> allowed)
    {
        if (forbidden.Count > 0 && forbidden.Contains(licenseId))
            return ViolationReason.Forbidden;

        if (allowed.Count > 0 && !allowed.Contains(licenseId))
            return ViolationReason.NotAllowed;

        return ViolationReason.None;
    }
}
