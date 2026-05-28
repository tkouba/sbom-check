using SbomCheck.Models;
using Spectre.Console;

namespace SbomCheck.Output;

static class LicenseSummaryRenderer
{
    public static void Render(LicensesResult result)
    {
        // Characters like ✔ and ✖ can cause issues in some terminals, so using text instead.
        String statusText = result.Status switch
        {
            LicenseStatus.Valid => "[green]Valid[/]: ",
            LicenseStatus.Invalid => "[red]Invalid[/]: ",
            LicenseStatus.Unknown => "[red]Invalid[/]: ", //"[dim]Unknown[/]: "
            _ => ""
        };
        AnsiConsole.MarkupLine($"{statusText}License summary");
        AnsiConsole.WriteLine();

        var sorted = result.LicenseDetails.OrderByDescending(ld => ld.Count);
        int maxLen = result.LicenseDetails.Max(ld => ld.LicenseId.Length);

        foreach (var item in sorted)
        {
            var paddedLicense = item.LicenseId.PadRight(maxLen);
            if (item.LicenseId == "UNKNOWN")
                AnsiConsole.MarkupLine($"  [dim]{Markup.Escape(paddedLicense)}[/]  [dim]{item.Count}[/]");
            else
                AnsiConsole.MarkupLine($"  {Markup.Escape(paddedLicense)}  {item.Count}");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine($"Total components found: {result.TotalComponents}");
        AnsiConsole.WriteLine();
    }
}
