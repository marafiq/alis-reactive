using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion;

/// <summary>
/// Plan-driven locators: the plan JSON tells us every component ID.
/// Zero hardcoded IDs. Zero knowledge of IdGenerator pattern.
/// The test writer only needs the MODEL PROPERTY NAME.
/// </summary>
[TestFixture]
public class WhenUsingPlanDrivenLocators : PlaywrightTestBase
{
    private const string Path = "/Sandbox/AutoComplete";

    private ReactivePlan _plan = null!;

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await ReactivePlan.FromPage(Page);
    }

    [Test]
    public async Task plan_discovers_all_components_on_page()
    {
        await NavigateAndBoot();

        // Plan knows every component — no hardcoded IDs
        Assert.That(_plan.ComponentNames, Does.Contain("Physician"));
        Assert.That(_plan.ComponentNames, Does.Contain("MedicationType"));

        // Each component has full metadata
        var physician = _plan.FindComponent("Physician")!;
        Assert.That(physician.Vendor, Is.EqualTo("fusion"));
        Assert.That(physician.ReadExpr, Is.EqualTo("value"));
        Assert.That(physician.Id, Does.Contain("Physician"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task selecting_physician_updates_echo()
    {
        await NavigateAndBoot();

        var physician = _plan.AutoComplete("Physician");

        // User gesture: open popup → select
        await Page.Locator("#show-popup-btn").ClickAsync();
        await physician.SelectItem("Dr. Johnson");

        // Assert what user sees
        await Expect(_plan.Element("change-value"))
            .ToContainTextAsync("johnson", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filtering_medication_populates_popup()
    {
        await NavigateAndBoot();

        var medication = _plan.AutoComplete("MedicationType");

        await medication.Type("anti");

        await Expect(_plan.Element("filter-status"))
            .ToHaveTextAsync("results loaded", new() { Timeout = 10000 });
        await Expect(medication.PopupItems.First)
            .ToBeVisibleAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task condition_fires_correctly_for_smith()
    {
        await NavigateAndBoot();

        var physician = _plan.AutoComplete("Physician");

        await Page.Locator("#show-popup-btn").ClickAsync();
        await physician.SelectItem("Dr. Smith");

        await Expect(_plan.Element("args-condition"))
            .ToHaveTextAsync("dr smith selected", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }
}
