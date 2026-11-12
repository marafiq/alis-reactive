using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextIsSetFromAnOptionalMemberItsHolderLacks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X3_0)]
    public async Task the_text_shows_nothing_and_the_step_after_it_runs()
    {
        var lacking = await NewWorld();
        await lacking.Page.GotoAsync(CarePages.RoundNote);
        await lacking.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "No text", Exact = true }).ClickAsync();
        await Expect(lacking.Page.Locator("#note-saved")).ToHaveTextAsync("Saved");
        await Expect(lacking.Page.Locator("#round-note")).ToHaveTextAsync("");

        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.RoundNote);
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ward round", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#round-note")).ToHaveTextAsync("Ward round");
    }
}
