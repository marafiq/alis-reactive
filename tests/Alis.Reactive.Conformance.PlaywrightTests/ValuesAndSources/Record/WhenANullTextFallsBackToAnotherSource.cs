using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANullTextFallsBackToAnotherSource(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_6_0)]
    public async Task a_null_text_shows_its_fallback_and_a_held_text_shows_itself()
    {
        var unnoted = await NewWorld();
        await unnoted.Page.GotoAsync(CarePages.AlarmNote);
        await unnoted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise a null text", Exact = true }).ClickAsync();
        await Expect(unnoted.Page.Locator("#note-text")).ToHaveTextAsync("Morning round");

        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.AlarmNote);
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise Line occluded", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#note-text")).ToHaveTextAsync("Line occluded");
    }
}
