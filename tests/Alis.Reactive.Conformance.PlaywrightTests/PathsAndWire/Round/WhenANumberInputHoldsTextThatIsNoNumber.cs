using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANumberInputHoldsTextThatIsNoNumber(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_3_0)]
    public async Task the_input_reads_its_start_and_its_check_fails_with_the_binders_message()
    {
        var mistyped = await NewWorld();
        await mistyped.Page.GotoAsync(CarePages.DoseOrdering);
        await mistyped.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Doses", Exact = true }).FillAsync("abc");
        await mistyped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(mistyped.Page.Locator("#dose-echo")).ToHaveTextAsync("5");
        await mistyped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(mistyped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Doses", Exact = true })
            .GetByText("The value 'abc' is not valid for Doses.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(mistyped.Page.Locator("#order-status")).ToHaveTextAsync("Not saved");

        var typed = await NewWorld();
        await typed.Page.GotoAsync(CarePages.DoseOrdering);
        await typed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Doses", Exact = true }).FillAsync("3");
        await typed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(typed.Page.Locator("#dose-echo")).ToHaveTextAsync("3");
        await typed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(typed.Page.Locator("#order-status")).ToHaveTextAsync("Saved 3");
        await Expect(typed.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
