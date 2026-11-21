using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.ConsumerNet48))]
[Category(Hosts.ConsumerNet48Name)]
public sealed class WhenAFreshNet48AppOpensItsHomePage(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X5_1)]
    public async Task its_plan_boots_and_writes_booted_over_what_the_server_rendered()
    {
        var person = await NewPerson();
        await person.GotoAsync(FreshPages.Home);
        await Expect(person.Locator("#consumer-status")).ToHaveTextAsync("Booted");

        var withJavaScriptOff = await NewPersonWithJavaScriptOff();
        await withJavaScriptOff.GotoAsync(FreshPages.Home);
        await Expect(withJavaScriptOff.Locator("#consumer-status")).ToHaveTextAsync("Not booted");
    }
}
