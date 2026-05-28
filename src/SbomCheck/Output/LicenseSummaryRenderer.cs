using SbomCheck.Models;
using Spectre.Console;

namespace SbomCheck.Output;

static class LicenseSummaryRenderer
{
    public static void Render(LicensesResult result, bool plain)
    {
        string statusLabel = result.Status switch
        {
            LicenseStatus.Valid   => "[green]Valid[/]",
            LicenseStatus.Invalid => "[red]Invalid[/]",
            LicenseStatus.Unknown => "[red]Invalid[/]",
            _                     => "Info"
        };

        Header($"{statusLabel}: License summary", plain);
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

        RenderIgnoredSection(result.IgnoredComponents, plain);

        RenderViolationSection(
            result.LicenseDetails.Where(ld => ld.ViolationReason == ViolationReason.Forbidden),
            "Forbidden licenses detected", plain);

        RenderViolationSection(
            result.LicenseDetails.Where(ld => ld.ViolationReason == ViolationReason.NotAllowed),
            "Licenses not in allowed list", plain);

        RenderComponentViolationSection(result.ComponentViolations, plain);
    }

    // In plain mode, MarkupLine strips all tags since AnsiConsole has Ansi=No configured.
    // In rich mode, Rule renders the text with box-drawing separators.
    static void Header(string markup, bool plain)
    {
        if (plain)
            AnsiConsole.MarkupLine(markup);
        else
            AnsiConsole.Write(new Rule(markup) { Justification = Justify.Left });
    }

    static void RenderIgnoredSection(List<IgnoredComponentInfo> ignored, bool plain)
    {
        if (ignored.Count == 0)
            return;

        Header($"[dim]Ignored components ({ignored.Count})[/]", plain);
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

    static void RenderComponentViolationSection(List<ComponentRuleViolation> violations, bool plain)
    {
        if (violations.Count == 0)
            return;

        Header("[red]Forbidden components detected[/]", plain);
        AnsiConsole.WriteLine();

        foreach (var rule in violations)
        {
            AnsiConsole.MarkupLine($"  [red]{Markup.Escape(rule.Display)}[/]");
            foreach (var c in rule.Components)
                AnsiConsole.MarkupLine($"    - {Markup.Escape(c.Name)}@{Markup.Escape(c.Version)}");
        }

        AnsiConsole.WriteLine();
    }

    static void RenderViolationSection(IEnumerable<LicenseDetail> violations, string header, bool plain)
    {
        var list = violations.OrderBy(ld => ld.LicenseId).ToList();
        if (list.Count == 0)
            return;

        Header($"[red]{Markup.Escape(header)}[/]", plain);
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
