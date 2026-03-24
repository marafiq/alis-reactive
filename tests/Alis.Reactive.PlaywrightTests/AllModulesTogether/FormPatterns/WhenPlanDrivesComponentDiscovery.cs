using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.AllModulesTogether.FormPatterns;

/// <summary>
/// Strongly-typed plan-driven locators.
///
/// View:  Html.InputField(plan, m => m.Physician, ...)
/// Test:  plan.AutoComplete(m => m.Physician)
///
/// Same expression. Same compile-time safety.
/// Rename Physician → PrimaryPhysician: both break at build.
/// </summary>
[TestFixture]
public class WhenPlanDrivesComponentDiscovery : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AutoComplete";

    private PagePlan<AutoCompleteModel> _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<AutoCompleteModel>.FromPage(Page);
    }

    [Test]
    public async Task plan_discovers_components_by_model_expression()
    {
        await NavigateAndBoot();

        // Expression-based lookup — compile-time checked
        var physician = _plan.FindComponent(m => m.Physician);
        Assert.That(physician, Is.Not.Null);
        Assert.That(physician!.Vendor, Is.EqualTo("fusion"));
        Assert.That(physician.ReadExpr, Is.EqualTo("value"));

        var medication = _plan.FindComponent(m => m.MedicationType);
        Assert.That(medication, Is.Not.Null);

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_physician_updates_echo()
    {
        await NavigateAndBoot();

        // Same expression as the view: m => m.Physician
        var physician = _plan.AutoComplete(m => m.Physician);

        await Page.Locator("#show-popup-btn").ClickAsync();
        await physician.SelectItem("Dr. Johnson");

        await Expect(_plan.Element("change-value"))
            .ToContainTextAsync("johnson", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_medication_populates_popup()
    {
        await NavigateAndBoot();

        var medication = _plan.AutoComplete(m => m.MedicationType);

        await medication.Type("anti");

        await Expect(_plan.Element("filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });
        await Expect(medication.PopupItems.First)
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task smith_condition_fires_then_branch()
    {
        await NavigateAndBoot();

        var physician = _plan.AutoComplete(m => m.Physician);

        await Page.Locator("#show-popup-btn").ClickAsync();
        await physician.SelectItem("Dr. Smith");

        await Expect(_plan.Element("args-condition"))
            .ToHaveTextAsync("dr smith selected", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
