using SbomCheck.Models;
using SbomCheck.Output;
using SbomCheck.Policy;
using SbomCheck.Sbom;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SbomCheck.Cli;

class CheckCommand : Command<CheckCommandSettings>
{
    protected override int Execute(CommandContext context, CheckCommandSettings settings, CancellationToken cancellationToken)
    {
        var bom = BomReader.TryRead(settings.BomFile, out var error);
        if (bom is null)
        {
            AnsiConsole.MarkupLine($"[red]Error[/]: {Markup.Escape(error!)}");
            return 1;
        }

        var forbiddenLicenses   = FlattenSimple(settings.ForbiddenLicenses);
        var allowedLicenses     = FlattenSimple(settings.AllowedLicenses);
        var forbiddenComponents = FlattenComponents(settings.ForbiddenComponents);

        var result = LicensePolicyEvaluator.Evaluate(bom, forbiddenLicenses, allowedLicenses);
        result.ComponentViolations = ComponentPolicyEvaluator.Evaluate(bom, forbiddenComponents);

        bool hasAnyPolicy    = forbiddenLicenses.Count > 0 || allowedLicenses.Count > 0 || forbiddenComponents.Count > 0;
        bool hasAnyViolation = result.LicenseDetails.Any(d => d.Status == LicenseStatus.Invalid)
                            || result.ComponentViolations.Count > 0;

        result.Status = !hasAnyPolicy   ? LicenseStatus.None
                      : hasAnyViolation ? LicenseStatus.Invalid
                      :                   LicenseStatus.Valid;

        LicenseSummaryRenderer.Render(result);

        return result.Status == LicenseStatus.Invalid ? 1 : 0;
    }

    // Simple comma split — safe for licenses (no commas in SPDX IDs)
    static List<string> FlattenSimple(string[] values) =>
        values.SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
              .ToList();

    // Bracket-aware split — preserves commas inside NuGet range notation, e.g. (-,2.0.1]
    static List<string> FlattenComponents(string[] values) =>
        values.SelectMany(SplitComponentRules).ToList();

    static IEnumerable<string> SplitComponentRules(string value)
    {
        var rules = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < value.Length; i++)
        {
            if      (value[i] is '(' or '[') depth++;
            else if (value[i] is ')' or ']') depth--;
            else if (value[i] == ',' && depth == 0)
            {
                var part = value[start..i].Trim();
                if (part.Length > 0) rules.Add(part);
                start = i + 1;
            }
        }

        var last = value[start..].Trim();
        if (last.Length > 0) rules.Add(last);

        return rules;
    }
}
