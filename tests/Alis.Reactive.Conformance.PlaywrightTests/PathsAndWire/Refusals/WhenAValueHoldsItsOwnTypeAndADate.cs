using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAValueHoldsItsOwnTypeAndADate(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X3_12_0)]
    public async Task the_view_is_refused_where_it_renders_and_told_to_pass_the_dates_as_a_list_of_their_own()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.Index);
        await refused.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the linked record" }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: A value written into a live member holds its own type and a date in it. "
                + "A date reaches a live member through a shape that ends: pass the dates as a list of their own.",
            Exact = true,
        })).ToBeVisibleAsync();

        var passed = await NewWorld();
        await passed.Page.GotoAsync(RefusalPages.Index);
        await passed.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the linked days" }).ClickAsync();
        await passed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Take the linked" }).ClickAsync();
        await Expect(passed.Page.GetByRole(AriaRole.Log, new PageGetByRoleOptions { Name = "Log" }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "Date 2026-03-01 08:00", "Date 2026-02-28 08:00" });
    }
}
