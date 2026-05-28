using System.ComponentModel;
using Spectre.Console.Cli;

namespace SbomCheck.Cli;

class CheckCommandSettings : CommandSettings
{
    [CommandArgument(0, "<bom.json>")]
    [Description("Path to CycloneDX bom.json")]
    public string BomFile { get; init; } = "";

    [CommandOption("--forbidden-licenses <licenses>")]
    [Description("Comma-separated SPDX license IDs to forbid (e.g. GPL-3.0,AGPL-3.0). Can be specified multiple times.")]
    public string[] ForbiddenLicenses { get; init; } = [];
}
