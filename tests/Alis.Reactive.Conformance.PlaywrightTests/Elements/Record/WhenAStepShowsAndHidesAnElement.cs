using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepShowsAndHidesAnElement(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X1_0)]
    public async Task a_note_hides_and_shows_and_a_step_on_an_element_the_page_lacks_stops_its_pipeline()
    {
        var record = await NewWorld();
        await record.Page.GotoAsync(CarePages.PinnedNote);
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hide the note", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#note-status")).ToHaveTextAsync("Hidden");
        await Expect(record.Page.Locator("#pinned-note")).ToBeHiddenAsync();
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the note", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#note-status")).ToHaveTextAsync("Shown");
        await Expect(record.Page.Locator("#pinned-note")).ToBeVisibleAsync();
        await record.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Stamp the note", Exact = true }).ClickAsync();
        await Expect(record.Page.Locator("#stamping")).ToHaveTextAsync("Stamping");
        await Expect(record.Page.Locator("#note-status")).ToHaveTextAsync("Shown");

        var shown = await NewWorld();
        await shown.Page.GotoAsync(CarePages.PinnedNote);
        await shown.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the note", Exact = true }).ClickAsync();
        await Expect(shown.Page.Locator("#note-status")).ToHaveTextAsync("Shown");
        await Expect(shown.Page.Locator("#pinned-note")).ToBeVisibleAsync();
    }
}
