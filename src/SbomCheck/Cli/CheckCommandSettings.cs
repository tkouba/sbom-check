using System.ComponentModel;
using Spectre.Console.Cli;

namespace SbomCheck.Cli;

class CheckCommandSettings : CommandSettings
{
    [CommandArgument(0, "<bom.json>")]
    [Description("Path to CycloneDX bom.json")]
    public string BomFile { get; init; } = "";
}
