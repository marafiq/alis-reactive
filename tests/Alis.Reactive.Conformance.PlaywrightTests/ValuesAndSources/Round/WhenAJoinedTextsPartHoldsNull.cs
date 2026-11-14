using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJoinedTextsPartHoldsNull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_17_0)]
    public async Task the_part_adds_nothing_and_the_parts_beside_it_are_joined()
    {
        var nameless = await NewWorld();
        await nameless.Page.GotoAsync(CarePages.DutyBoardWithANurseWithNoName);
        await nameless.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the nurse", Exact = true }).ClickAsync();
        await Expect(nameless.Page.Locator("#duty-line")).ToHaveTextAsync("Nurse [] on duty");

        var bea = await NewWorld();
        await bea.Page.GotoAsync(CarePages.DutyBoardWithBeaOnDuty);
        await bea.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find the nurse", Exact = true }).ClickAsync();
        await Expect(bea.Page.Locator("#duty-line")).ToHaveTextAsync("Nurse [Bea] on duty");
    }
}
