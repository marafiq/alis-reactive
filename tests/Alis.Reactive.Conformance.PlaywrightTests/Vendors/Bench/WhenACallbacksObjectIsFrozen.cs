using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACallbacksObjectIsFrozen(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F1.X17_51_0)]
    public async Task a_refused_assignment_stops_its_listen_and_a_refused_put_back_names_its_object()
    {
        var frozen = await NewWorld();
        await frozen.Page.GotoAsync(BenchPages.VendorsFrozenCallbacks);
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Freeze A", Exact = true }).ClickAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open card A", Exact = true }).ClickAsync();
        await Expect(frozen.Page.GetByText("Card A listens to wing A's calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise A", Exact = true }).ClickAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the notes", Exact = true }).ClickAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Listen to A", Exact = true }).ClickAsync();
        await Expect(frozen.Page.Locator("#listen-tried")).ToHaveTextAsync("listening");
        await Expect(frozen.Page.Locator("#listen-done")).ToHaveTextAsync("");
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open card B", Exact = true }).ClickAsync();
        await Expect(frozen.Page.GetByText("Card B listens to wing B's calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Freeze B", Exact = true }).ClickAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise B", Exact = true }).ClickAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close card B", Exact = true }).ClickAsync();
        await Expect(frozen.Page.GetByText("Card B listens to wing B's calls.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await frozen.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise B", Exact = true }).ClickAsync();
        await Expect(frozen.Page.Locator("#callbacks-a").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "callbacks-a" });
        await Expect(frozen.Page.Locator("#callbacks-b").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "callbacks-b card-b", "callbacks-b" });
        Assert.That(frozen.ConsoleErrors, Is.EqualTo(new[] { "The listen to 'onRaised' on the element 'callbacks-a' of the vendor 'conformance-callbacks' did not start: The object refused the callback property 'onRaised'.", "The listen to 'onRaised' on the element 'callbacks-a' of the vendor 'conformance-callbacks' did not start: The object refused the callback property 'onRaised'.", "Releasing the slot 'card-b' threw, and every owner was released past it: the slot 'card-b' threw while removing its listeners (The listen to 'onRaised' on the element 'callbacks-b' of the vendor 'conformance-callbacks' was not removed: The object refused the callback property 'onRaised'.)" }));

        var open = await NewWorld();
        await open.Page.GotoAsync(BenchPages.VendorsFrozenCallbacks);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open card A", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByText("Card A listens to wing A's calls.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise A", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the notes", Exact = true }).ClickAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Listen to A", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#listen-done")).ToHaveTextAsync("listened");
        await Expect(open.Page.Locator("#callbacks-a").GetByRole(AriaRole.Log, new LocatorGetByRoleOptions { Name = "Log", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "callbacks-a card-a" });
    }
}
