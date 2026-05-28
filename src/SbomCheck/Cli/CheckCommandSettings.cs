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

    [CommandOption("--allowed-licenses <licenses>")]
    [Description("Comma-separated SPDX license IDs that are permitted (e.g. MIT,Apache-2.0). Any other license is a violation. Can be specified multiple times.")]
    public string[] AllowedLicenses { get; init; } = [];

    [CommandOption("--forbidden-components <components>")]
    [Description("Comma-separated package names to forbid (case-insensitive, e.g. log4net,Newtonsoft.Json). Can be specified multiple times.")]
    public string[] ForbiddenComponents { get; init; } = [];

    [CommandOption("--ignore-components <components>")]
    [Description("Components to exclude from all policy checks. Supports wildcards (Microsoft*) and version ranges (log4net@(-,2.0.1]). Takes priority over forbidden rules. Can be specified multiple times.")]
    public string[] IgnoreComponents { get; init; } = [];

    [CommandOption("--no-color")]
    [Description("Disable ANSI color and styling in output.")]
    public bool NoColor { get; init; }
}
