using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANumberInputIsLeftBlank(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_4_0)]
    public async Task the_blank_sends_and_validates_as_the_members_start()
    {
        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.RepeatDoseOrder);
        await blank.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Doses", Exact = true }).ClearAsync();
        await blank.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(blank.Page.Locator("#order-status")).ToHaveTextAsync("Saved 5");
        await Expect(blank.Page.GetByText("Give more than 4 doses.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var tooFew = await NewWorld();
        await tooFew.Page.GotoAsync(CarePages.RepeatDoseOrder);
        await tooFew.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Doses", Exact = true }).FillAsync("2");
        await tooFew.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(tooFew.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Doses", Exact = true })
            .GetByText("Give more than 4 doses.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(tooFew.Page.Locator("#order-status")).ToHaveTextAsync("Not saved");
    }
}
