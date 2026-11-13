using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheInputASaveSentIsReloadedBeforeItsAnswer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_19_0)]
    public async Task the_message_shows_in_the_summary_and_nothing_shows_beside_the_reloaded_input()
    {
        var reloaded = await NewWorld();
        await reloaded.Page.GotoAsync(CarePages.RoomSave);
        await reloaded.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true }).FillAsync("13");
        var armed = await reloaded.Arm(OwnershipGate.ShowsFailures);
        await reloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(reloaded.Page.Locator("#room-status")).ToHaveTextAsync("Saving…");
        await reloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reload the room", Exact = true }).ClickAsync();
        await Expect(reloaded.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true })).ToBeEmptyAsync();
        await held.Release();
        await Expect(reloaded.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Room 13 is closed." });
        await Expect(reloaded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true })
            .GetByText("Room 13 is closed.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.RoomSave);
        await kept.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room", Exact = true }).FillAsync("13");
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true })
            .GetByText("Room 13 is closed.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Room 13 is closed." });
    }
}
