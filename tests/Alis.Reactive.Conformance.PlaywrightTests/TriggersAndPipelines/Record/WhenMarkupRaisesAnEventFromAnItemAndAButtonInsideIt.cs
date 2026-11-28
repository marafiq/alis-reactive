using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenMarkupRaisesAnEventFromAnItemAndAButtonInsideIt(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F2.M1_X4_0)]
    public async Task each_click_raises_once_with_the_payload_of_the_nearest_marked_element_its_numbers_compared_as_numbers()
    {
        var visitors = await NewWorld();
        await visitors.Page.GotoAsync(CarePages.VisitorList);
        await visitors.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Driver", Exact = true }).ClickAsync();
        await Expect(visitors.Page.Locator("#announced-type")).ToHaveTextAsync("a visitor");
        await Expect(visitors.Page.Locator("#announced-size")).ToHaveTextAsync("1");
        await Expect(visitors.Page.Locator("#announced-capacity")).ToHaveTextAsync("Within capacity");
        await visitors.Page.GetByText("Coach party of twelve", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(visitors.Page.Locator("#announced-type")).ToHaveTextAsync("a coach party");
        await Expect(visitors.Page.Locator("#announced-size")).ToHaveTextAsync("12");
        await Expect(visitors.Page.Locator("#announced-capacity")).ToHaveTextAsync("Over capacity");
        await visitors.Page.GetByText("Choose a party to announce it at the gate.", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(visitors.Page.Locator("#announced-type")).ToHaveTextAsync("a coach party");
        await Expect(visitors.Page.Locator("#announced-size")).ToHaveTextAsync("12");
        await Expect(visitors.Page.Locator("#announced-capacity")).ToHaveTextAsync("Over capacity");

        var family = await NewWorld();
        await family.Page.GotoAsync(CarePages.VisitorListOfASmallParty);
        await family.Page.GetByText("Family of four", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(family.Page.Locator("#announced-capacity")).ToHaveTextAsync("Within capacity");
    }
}
