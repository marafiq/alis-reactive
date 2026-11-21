using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMentionsPopupIsHeardAtTheViewsId(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F29.M21_X1034_0)]
    public async Task the_views_handlers_hear_the_popup_open_and_close()
    {
        var suggesting = await NewWorld();
        await suggesting.Page.GotoAsync(CarePages.StaffMention);
        await suggesting.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Note", Exact = true }).GetByRole(AriaRole.Textbox).PressSequentiallyAsync("@Li");

        await Expect(suggesting.Page.Locator("#mention-state")).ToHaveTextAsync("Suggesting staff");

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.StaffMention);
        await chosen.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Note", Exact = true }).GetByRole(AriaRole.Textbox).PressSequentiallyAsync("@Li");
        await Expect(chosen.Page.Locator("#mention-state")).ToHaveTextAsync("Suggesting staff");
        await chosen.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Dr. Lindqvist", Exact = true }).ClickAsync();

        await Expect(chosen.Page.Locator("#mention-state")).ToHaveTextAsync("Writing");
    }
}
