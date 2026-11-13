using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextBoxIsRenderedForAMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F49.M21_X1030_0)]
    public async Task the_box_starts_at_the_text_the_member_holds()
    {
        var given = await NewWorld();
        await given.Page.GotoAsync(CarePages.ProfileNameGiven);

        await Expect(given.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred name", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("Nell");

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.ProfileNameBlank);

        await Expect(blank.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Preferred name", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("");
    }
}
