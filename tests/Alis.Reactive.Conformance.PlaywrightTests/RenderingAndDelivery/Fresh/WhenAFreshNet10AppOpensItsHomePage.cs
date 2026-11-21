using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.ConsumerNet10))]
[Category(Hosts.ConsumerNet10Name)]
public sealed class WhenAFreshNet10AppOpensItsHomePage(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X5_0)]
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
