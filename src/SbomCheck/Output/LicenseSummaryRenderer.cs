using SbomCheck.Models;
using Spectre.Console;

namespace SbomCheck.Output;

static class LicenseSummaryRenderer
{
    public static void Render(LicensesResult result)
    {
        // Characters like ✔ and ✖ can cause issues in some terminals, so using text instead.
        string statusText = result.Status switch
        {
            LicenseStatus.Valid   => "[green]Valid[/]: ",
            LicenseStatus.Invalid => "[red]Invalid[/]: ",
            LicenseStatus.Unknown => "[red]Invalid[/]: ",
            _                     => ""
        };
        AnsiConsole.MarkupLine($"{statusText}License summary");
        AnsiConsole.WriteLine();

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

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine($"Total components found: {result.TotalComponents}");
        AnsiConsole.WriteLine();

        RenderViolationSection(
            result.LicenseDetails.Where(ld => ld.ViolationReason == ViolationReason.Forbidden),
            "Forbidden licenses detected:");

        RenderViolationSection(
            result.LicenseDetails.Where(ld => ld.ViolationReason == ViolationReason.NotAllowed),
            "Licenses not in allowed list:");
    }

    static void RenderViolationSection(IEnumerable<LicenseDetail> violations, string header)
    {
        var list = violations.OrderBy(ld => ld.LicenseId).ToList();
        if (list.Count == 0)
            return;

        AnsiConsole.MarkupLine($"[red]{header}[/]");
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
