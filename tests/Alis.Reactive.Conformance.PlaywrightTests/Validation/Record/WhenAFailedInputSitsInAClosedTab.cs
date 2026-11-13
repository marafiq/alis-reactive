using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFailedInputSitsInAClosedTab(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_7_0)]
    public async Task the_message_shows_in_the_summary_while_its_tab_is_closed()
    {
        var personal = await NewWorld();
        await personal.Page.GotoAsync(CarePages.CarePlan);
        await personal.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(personal.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "List allergies or none." });

        var medical = await NewWorld();
        await medical.Page.GotoAsync(CarePages.CarePlan);
        await medical.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await medical.Page.GetByText("Medical", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(medical.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Allergies", Exact = true })
            .GetByText("List allergies or none.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(medical.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "List allergies or none." });
    }
}
