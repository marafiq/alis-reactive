using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextIsSetFromAMemberThePayloadLacks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_5_0)]
    public async Task the_text_shows_its_empty_state()
    {
        var lacking = await NewWorld();
        await lacking.Page.GotoAsync(CarePages.FallNote);
        await lacking.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise without optional text", Exact = true }).ClickAsync();
        await Expect(lacking.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(lacking.Page.Locator("#note-text")).ToHaveTextAsync("");

        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.FallNote);
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise with optional text", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#note-text")).ToHaveTextAsync("Fall at 3am");
    }
}
