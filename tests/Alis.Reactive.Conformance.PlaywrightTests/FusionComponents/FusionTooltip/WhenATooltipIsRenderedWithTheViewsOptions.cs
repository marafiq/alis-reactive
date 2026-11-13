using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATooltipIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F52.M21_X1033_0)]
    public async Task a_step_opens_the_tip_at_its_id_showing_the_views_content()
    {
        var penicillin = await NewWorld();
        await penicillin.Page.GotoAsync(CarePages.AllergyTipOfPenicillin);
        await penicillin.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the tip", Exact = true }).ClickAsync();

        await Expect(penicillin.Page.GetByRole(AriaRole.Tooltip)).ToHaveTextAsync("Allergic to penicillin");

        var none = await NewWorld();
        await none.Page.GotoAsync(CarePages.AllergyTipOfNone);
        await none.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the tip", Exact = true }).ClickAsync();

        await Expect(none.Page.GetByRole(AriaRole.Tooltip)).ToHaveTextAsync("No known allergies");
    }
}
