using SbomCheck.Models;
using SbomCheck.Output;
using Spectre.Console;

namespace SbomCheck.Tests;

public class LicenseSummaryRendererTests
{
    [Fact]
    public void Render_NoMessage_UsesDefaultTitle()
    {
        var output = Capture(result => LicenseSummaryRenderer.Render(result, plain: true));

        Assert.Contains("Valid: License summary", output);
    }

    [Fact]
    public void Render_WithMessage_ReplacesDefaultTitle()
    {
        var output = Capture(result => LicenseSummaryRenderer.Render(result, plain: true, message: "Dependency policy check"));

        Assert.Contains("Valid: Dependency policy check", output);
        Assert.DoesNotContain("License summary", output);
    }

    [Fact]
    public void Render_EmptyMessage_FallsBackToDefaultTitle()
    {
        var output = Capture(result => LicenseSummaryRenderer.Render(result, plain: true, message: ""));

        Assert.Contains("Valid: License summary", output);
    }

    [Fact]
    public void Render_MessageWithMarkup_IsEscapedNotInterpreted()
    {
        var output = Capture(result => LicenseSummaryRenderer.Render(result, plain: true, message: "[red]inject[/]"));

        Assert.Contains("Valid: [red]inject[/]", output);
    }

    static string Capture(Action<LicensesResult> render)
    {
        var writer = new StringWriter();
        var previous = AnsiConsole.Console;
        AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi        = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Out         = new AnsiConsoleOutput(writer),
        });

        try
        {
            render(new LicensesResult { Status = LicenseStatus.Valid });
            return writer.ToString();
        }
        finally
        {
            AnsiConsole.Console = previous;
        }
    }
}
