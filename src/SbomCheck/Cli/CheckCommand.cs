using SbomCheck.Models;
using SbomCheck.Output;
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

        var licenseCounts = bom.Components
            .SelectMany(c => c.GetLicenseIds())
            .GroupBy(id => id, StringComparer.OrdinalIgnoreCase)
            .Select(g => new LicenseDetail { LicenseId = g.Key, Count = g.Count() });

        var result = new LicensesResult
        {
            Status = LicenseStatus.None, // Placeholder, real status calculation would go here
            TotalComponents = bom.Components.Count,
            LicenseDetails = licenseCounts.ToList()
        };

        LicenseSummaryRenderer.Render(result);
        return 0;
    }
}
