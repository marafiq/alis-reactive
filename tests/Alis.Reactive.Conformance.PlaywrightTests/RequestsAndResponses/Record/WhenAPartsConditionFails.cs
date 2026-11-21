using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPartsConditionFails(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X10_10_0)]
    public async Task the_part_is_left_out_and_the_endpoint_binds_the_members_start()
    {
        var leftOut = await NewWorld();
        await leftOut.Page.GotoAsync(CarePages.WellbeingCheck);
        await leftOut.Page.GetByLabel("Note", new PageGetByLabelOptions { Exact = true }).FillAsync("late");
        await leftOut.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(leftOut.Page.Locator("#note-echo")).ToHaveTextAsync("none");

        var included = await NewWorld();
        await included.Page.GotoAsync(CarePages.WellbeingCheck);
        await included.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Include note", Exact = true }).CheckAsync();
        await included.Page.GetByLabel("Note", new PageGetByLabelOptions { Exact = true }).FillAsync("late");
        await included.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(included.Page.Locator("#note-echo")).ToHaveTextAsync("late");
    }
}
