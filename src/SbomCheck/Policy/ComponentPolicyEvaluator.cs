using SbomCheck.Models;
using SbomCheck.Sbom.Models;

namespace SbomCheck.Policy;

static class ComponentPolicyEvaluator
{
    public static List<ComponentRuleViolation> Evaluate(BomDocument bom, IReadOnlyCollection<string> forbiddenComponents)
    {
        if (forbiddenComponents.Count == 0)
            return [];

        var rules = forbiddenComponents.Select(ComponentRule.Parse).ToList();
        var violations = new List<ComponentRuleViolation>();

        foreach (var rule in rules)
        {
            var matching = bom.Components
                .Where(c => rule.Matches(c.Name, c.Version))
                .Select(c => new ComponentViolation(c.Name, c.Version ?? ""))
                .ToList();

            if (matching.Count > 0)
                violations.Add(new ComponentRuleViolation { Display = rule.Display, Components = matching });
        }

        return violations;
    }
}
