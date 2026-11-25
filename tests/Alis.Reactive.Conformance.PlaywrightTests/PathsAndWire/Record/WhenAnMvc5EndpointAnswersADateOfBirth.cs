using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenAnMvc5EndpointAnswersADateOfBirth(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_7_0)]
    public async Task the_date_of_birth_shows_its_own_day_in_a_browser_whose_clock_is_behind_the_servers()
    {
        var card = await NewWorldInZone(Zones.NewYork);
        await card.Seed(Seed.TheResidentBornOn(1990, 7, 14));
        await card.Page.GotoAsync(CarePages.ResidentCard);
        await card.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(card.Page.Locator("#dob")).ToHaveTextAsync("1990-07-14");

        var jsonResultCard = await NewWorldInZone(Zones.NewYork);
        await jsonResultCard.Seed(Seed.TheResidentBornOn(1990, 7, 14));
        await jsonResultCard.Page.GotoAsync(CarePages.ResidentCardAnsweredByJsonResult);
        await Expect(jsonResultCard.Page.Locator("#dob")).ToHaveTextAsync("Not loaded");
        await jsonResultCard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(jsonResultCard.Page.Locator("#dob")).ToHaveTextAsync("");
    }
}
