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

    [Fact]
    public void Render_ShortSummary_NoViolations_ShowsTotalInHeaderOnly()
    {
        var result = new LicensesResult
        {
            Status = LicenseStatus.None,
            TotalComponents = 16,
            LicenseDetails =
            [
                new LicenseDetail { LicenseId = "MIT", Count = 16, Status = LicenseStatus.Valid }
            ]
        };

        var output = Capture(r => LicenseSummaryRenderer.Render(r, plain: true, shortSummary: true), result);

        Assert.Contains("Info: Total components found: 16", output);
        Assert.DoesNotContain("MIT", output);
        Assert.DoesNotContain("License summary", output);
    }

    [Fact]
    public void Render_ShortSummary_SuppressesLicenseListAndTotalsLine()
    {
        var result = new LicensesResult
        {
            Status = LicenseStatus.Invalid,
            TotalComponents = 2,
            LicenseDetails =
            [
                new LicenseDetail
                {
                    LicenseId = "GPL-3.0",
                    Count = 1,
                    Status = LicenseStatus.Invalid,
                    ViolationReason = ViolationReason.Forbidden,
                    Components = [new LicenseComponent("Some.Package", "1.0.0")]
                }
            ]
        };

        var output = Capture(r => LicenseSummaryRenderer.Render(r, plain: true, shortSummary: true), result);

        Assert.DoesNotContain("GPL-3.0  1", output);
        Assert.Contains("Forbidden licenses detected", output);
        Assert.Contains("Some.Package@1.0.0", output);
    }

    [Fact]
    public void Render_ShortSummary_SuppressesIgnoredComponentsSection()
    {
        var result = new LicensesResult
        {
            Status = LicenseStatus.Valid,
            TotalComponents = 3,
            IgnoredComponents = [new IgnoredComponentInfo("Legacy.Component", "1.0.0", ["MIT"])]
        };

        var output = Capture(r => LicenseSummaryRenderer.Render(r, plain: true, shortSummary: true), result);

        Assert.DoesNotContain("Ignored components", output);
        Assert.DoesNotContain("Legacy.Component", output);
    }

    [Fact]
    public void Render_ShortSummary_WithMessage_MessageTakesPriority()
    {
        var output = Capture(r => LicenseSummaryRenderer.Render(r, plain: true, message: "Custom check", shortSummary: true));

        Assert.Contains("Valid: Custom check", output);
        Assert.DoesNotContain("Total components found", output);
    }

    [Fact]
    public void Render_ShortSummary_WithMessage_Invalid_ShowsMessageAndViolationsOnly()
    {
        var result = new LicensesResult
        {
            Status = LicenseStatus.Invalid,
            TotalComponents = 5,
            LicenseDetails =
            [
                new LicenseDetail { LicenseId = "MIT", Count = 4, Status = LicenseStatus.Valid }
            ],
            ComponentViolations =
            [
                new ComponentRuleViolation
                {
                    Display = "InTheHand.Bluetooth",
                    Components = [new ComponentViolation("InTheHand.Bluetooth", "5.1.2")]
                }
            ]
        };

        var output = Capture(r => LicenseSummaryRenderer.Render(r, plain: true, message: "Dependency policy check", shortSummary: true), result);

        Assert.Contains("Invalid: Dependency policy check", output);
        Assert.DoesNotContain("License summary", output);
        Assert.DoesNotContain("Total components found", output);
        Assert.DoesNotContain("MIT", output);
        Assert.Contains("Forbidden components detected", output);
        Assert.Contains("InTheHand.Bluetooth@5.1.2", output);
    }

    static string Capture(Action<LicensesResult> render, LicensesResult? result = null)
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
            render(result ?? new LicensesResult { Status = LicenseStatus.Valid });
            return writer.ToString();
        }
        finally
        {
            AnsiConsole.Console = previous;
        }
    }
}
