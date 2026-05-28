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

        var forbiddenLicenses = settings.ForbiddenLicenses
            .SelectMany(v => v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        var result = LicensePolicyEvaluator.Evaluate(bom, forbiddenLicenses);

        LicenseSummaryRenderer.Render(result);

        return result.Status == Models.LicenseStatus.Invalid ? 1 : 0;
    }
}
