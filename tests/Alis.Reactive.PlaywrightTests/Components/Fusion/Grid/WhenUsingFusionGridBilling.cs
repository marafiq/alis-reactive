using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

[TestFixture]
public class WhenUsingFusionGridBilling : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/Billing";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentBillingViewModel";
    private const string GridId = "billing-grid";

    private FusionTextBoxLocator NewResidentName => new(Page, GeneratedTypeScope + "__NewResidentName");
    private DropDownListLocator NewCareLevel => new(Page, GeneratedTypeScope + "__NewCareLevel");
    private NumericTextBoxLocator NewMonthlyRate => new(Page, GeneratedTypeScope + "__NewMonthlyRate");
    private NumericTextBoxLocator NewAddOnCharges => new(Page, GeneratedTypeScope + "__NewAddOnCharges");

    private ILocator ErrorFor(string property) => Page.Locator($"#{GeneratedTypeScope}__{property}_error");
    private ILocator FirstRowCell(int columnIndex) =>
        Page.Locator($"#{GridId} .e-row").First.Locator(".e-rowcell").Nth(columnIndex);

    private async Task NavigateBilling()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        await Expect(Page.Locator("#billing-status"))
            .ToHaveTextAsync("loaded current census", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId} .e-row").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task billing_board_loads_census_with_currency_and_outstanding_summary()
    {
        await NavigateBilling();

        await Expect(Page.Locator($"#{GridId}")).ToContainTextAsync("Amina Patel", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).ToContainTextAsync("$3,200", new() { Timeout = 10000 });
        await Expect(Page.Locator("#billing-summary")).ToContainTextAsync("outstanding", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task applying_five_percent_increase_to_selected_resident_raises_their_rate()
    {
        await NavigateBilling();

        await ClickWhenStable(FirstRowCell(1));
        await ClickWhenStable(Page.Locator("#billing-bulk-increase"));

        await Expect(Page.Locator("#billing-summary"))
            .ToHaveTextAsync("applied 5% increase to 1 resident(s)", new() { Timeout = 10000 });
        await Expect(Page.Locator("#billing-status"))
            .ToHaveTextAsync("rate increase applied", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).ToContainTextAsync("$3,360", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_batch_cell_edits_posts_all_charges_to_the_server()
    {
        await NavigateBilling();

        await FirstRowCell(4).DblClickAsync();
        await Expect(Page.Locator($"#{GridId} input.e-field").First)
            .ToBeVisibleAsync(new() { Timeout = 10000 });
        await Page.Locator($"#{GridId} input.e-field").First.FillAsync("5000");
        await Page.Keyboard.PressAsync("Enter");

        await ClickWhenStable(Page.Locator("#billing-save-all"));

        await Expect(Page.Locator("#billing-summary"))
            .ToContainTextAsync("saved 1 resident charge(s)", new() { Timeout = 10000 });
        await Expect(Page.Locator("#billing-status"))
            .ToHaveTextAsync("charges saved", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).ToContainTextAsync("$5,000", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_a_charge_through_the_dialog_validates_then_inserts_a_row()
    {
        await NavigateBilling();

        await ClickWhenStable(Page.Locator("#billing-add-open"));
        await Expect(Page.Locator("#add-charge-dialog")).Not.ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("hidden"), new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#add-charge-save"));
        await Expect(ErrorFor("NewResidentName")).ToContainTextAsync("required", new() { Timeout = 10000 });
        await Expect(ErrorFor("NewCareLevel")).ToContainTextAsync("required", new() { Timeout = 10000 });

        await NewResidentName.FillAndBlur("Dorothy Admit");
        await NewCareLevel.Select("Assisted Living");
        await NewMonthlyRate.FillAndBlur("5200");
        await NewAddOnCharges.FillAndBlur("250");
        await ClickWhenStable(Page.Locator("#add-charge-save"));

        await Expect(Page.Locator("#billing-status")).ToHaveTextAsync("charge added", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).ToContainTextAsync("Dorothy Admit", new() { Timeout = 10000 });
        await Expect(Page.Locator("#add-charge-dialog")).ToHaveClassAsync(
            new System.Text.RegularExpressions.Regex("hidden"), new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_email_statements_toolbar_item_runs_the_toolbar_workflow()
    {
        await NavigateBilling();

        await ClickWhenStable(
            Page.Locator($"#{GridId} .e-toolbar-item").Filter(new() { HasText = "Email Statements" }));

        await Expect(Page.Locator("#toolbar-status"))
            .ToHaveTextAsync("statements queued for emailing", new() { Timeout = 10000 });
        await Expect(Page.Locator("#toolbar-item-id"))
            .ToHaveTextAsync("emailStatements", new() { Timeout = 10000 });
        await Expect(Page.Locator("#toolbar-item-text"))
            .ToHaveTextAsync("Email Statements", new() { Timeout = 10000 });
        await Expect(Page.Locator("#toolbar-cancel"))
            .ToHaveTextAsync("false", new() { Timeout = 10000 });
        await Expect(Page.Locator("#toolbar-event"))
            .ToHaveTextAsync("toolbarClick", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task memory_care_quick_filter_reloads_only_memory_care_residents()
    {
        await NavigateBilling();

        await ClickWhenStable(Page.Locator("#billing-quick-memory"));

        await Expect(Page.Locator("#billing-summary"))
            .ToContainTextAsync("Memory Care", new() { Timeout = 10000 });
        await Expect(Page.Locator("#billing-status"))
            .ToHaveTextAsync("showing memory care residents", new() { Timeout = 10000 });
        await Expect(Page.Locator($"#{GridId}")).Not.ToContainTextAsync("Independent", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task roster_tools_autofit_and_column_chooser_run_through_onboarded_methods()
    {
        await NavigateBilling();

        await ClickWhenStable(Page.Locator("#billing-autofit"));
        await Expect(Page.Locator("#tool-status"))
            .ToHaveTextAsync("columns auto-fitted", new() { Timeout = 10000 });

        await ClickWhenStable(Page.Locator("#billing-columns"));
        await Expect(Page.Locator("#tool-status"))
            .ToHaveTextAsync("column chooser opened", new() { Timeout = 10000 });
        await Expect(Page.Locator(".e-ccdlg").First).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
