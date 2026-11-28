using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.ConsumerNet10))]
[Category(Hosts.ConsumerNet10Name)]
public sealed class WhenAFreshNet10AppsOwnButtonIsClicked(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X7_0)]
    public async Task the_button_keeps_its_text_until_its_click_sets_the_text_the_view_names()
    {
        var clicksNothing = await NewPerson();
        await clicksNothing.GotoAsync(FreshPages.Home);
        await Expect(clicksNothing.Locator("#consumer-status")).ToHaveTextAsync("Booted");
        await Expect(clicksNothing.Locator("#acknowledge")).ToHaveTextAsync("Acknowledge");

        var person = await NewPerson();
        await person.GotoAsync(FreshPages.Home);
        await person.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Acknowledge", Exact = true }).ClickAsync();
        await Expect(person.Locator("#acknowledge")).ToHaveTextAsync("Acknowledged");
    }
}
