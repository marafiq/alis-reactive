using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACascadesFirstAnswerArrivesAfterItsSecond(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F1.X11_6_0)]
    public async Task under_replace_the_cascade_shows_the_seconds_items()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.RoomChoiceUnderReplace);
        var armed = await replaced.Arm(LanesGate.Respond, CareEndpoints.RoomChoiceRooms);
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        var held = await armed.Held();
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("West");
        await Expect(replaced.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "W1", "W2", "W3" });
        await held.Release();
        await Expect(replaced.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "W1", "W2", "W3" });

        var overlapped = await NewWorld();
        await overlapped.Page.GotoAsync(CarePages.RoomChoiceUnderOverlap);
        var armedAgain = await overlapped.Arm(LanesGate.Respond, CareEndpoints.RoomChoiceRooms);
        await overlapped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("East");
        var heldAgain = await armedAgain.Held();
        await overlapped.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("West");
        await Expect(overlapped.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "W1", "W2", "W3" });
        await heldAgain.Release();
        await Expect(overlapped.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "E1", "E2" });
    }
}
