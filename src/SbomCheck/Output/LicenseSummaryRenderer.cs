using SbomCheck.Models;
using Spectre.Console;

namespace SbomCheck.Output;

static class LicenseSummaryRenderer
{
    public static void Render(LicensesResult result)
    {
        string statusLabel = result.Status switch
        {
            LicenseStatus.Valid   => "[green]Valid[/]",
            LicenseStatus.Invalid => "[red]Invalid[/]",
            LicenseStatus.Unknown => "[red]Invalid[/]",
            _                     => "Info"
        };

        AnsiConsole.Write(new Rule($"{statusLabel}: License summary") { Justification = Justify.Left });
        AnsiConsole.WriteLine();

        if (result.LicenseDetails.Count > 0)
        {
            var sorted = result.LicenseDetails.OrderByDescending(ld => ld.Count);
            int maxLen = result.LicenseDetails.Max(ld => ld.LicenseId.Length);

            foreach (var item in sorted)
            {
                var paddedLicense = item.LicenseId.PadRight(maxLen);
                if (item.Status == LicenseStatus.Invalid)
                    AnsiConsole.MarkupLine($"  [red]{Markup.Escape(paddedLicense)}[/]  [red]{item.Count}[/]");
                else if (item.LicenseId == "UNKNOWN")
                    AnsiConsole.MarkupLine($"  [dim]{Markup.Escape(paddedLicense)}[/]  [dim]{item.Count}[/]");
                else
                    AnsiConsole.MarkupLine($"  {Markup.Escape(paddedLicense)}  {item.Count}");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine($"Total components found: {result.TotalComponents}");
        AnsiConsole.WriteLine();

        RenderIgnoredSection(result.IgnoredComponents);

        RenderViolationSection(
            result.LicenseDetails.Where(ld => ld.ViolationReason == ViolationReason.Forbidden),
            "Forbidden licenses detected");

        RenderViolationSection(
            result.LicenseDetails.Where(ld => ld.ViolationReason == ViolationReason.NotAllowed),
            "Licenses not in allowed list");

        RenderComponentViolationSection(result.ComponentViolations);
    }

    static void RenderIgnoredSection(List<IgnoredComponentInfo> ignored)
    {
        if (ignored.Count == 0)
            return;

        AnsiConsole.Write(new Rule($"[dim]Ignored components ({ignored.Count})[/]") { Justification = Justify.Left });
        AnsiConsole.WriteLine();

        int maxRef = ignored.Max(c => (c.Name + "@" + c.Version).Length);

        foreach (var c in ignored)
        {
            var componentRef = $"{c.Name}@{c.Version}".PadRight(maxRef);
            var licenses     = string.Join(", ", c.Licenses);
            AnsiConsole.MarkupLine($"  [dim]{Markup.Escape(componentRef)}  {Markup.Escape(licenses)}[/]");
        }

        AnsiConsole.WriteLine();
    }

    static void RenderComponentViolationSection(List<ComponentRuleViolation> violations)
    {
        if (violations.Count == 0)
            return;

        AnsiConsole.Write(new Rule("[red]Forbidden components detected[/]") { Justification = Justify.Left });
        AnsiConsole.WriteLine();

        foreach (var rule in violations)
        {
            AnsiConsole.MarkupLine($"  [red]{Markup.Escape(rule.Display)}[/]");
            foreach (var c in rule.Components)
                AnsiConsole.MarkupLine($"    - {Markup.Escape(c.Name)}@{Markup.Escape(c.Version)}");
        }

        AnsiConsole.WriteLine();
    }

    static void RenderViolationSection(IEnumerable<LicenseDetail> violations, string header)
    {
        var list = violations.OrderBy(ld => ld.LicenseId).ToList();
        if (list.Count == 0)
            return;

        AnsiConsole.Write(new Rule($"[red]{Markup.Escape(header)}[/]") { Justification = Justify.Left });
        AnsiConsole.WriteLine();

        foreach (var violation in list)
        {
            AnsiConsole.MarkupLine($"  [red]{Markup.Escape(violation.LicenseId)}[/]");
            foreach (var component in violation.Components)
                AnsiConsole.MarkupLine($"    - {Markup.Escape(component.Name)}@{Markup.Escape(component.Version)}");
        }

        AnsiConsole.WriteLine();
    }
}
