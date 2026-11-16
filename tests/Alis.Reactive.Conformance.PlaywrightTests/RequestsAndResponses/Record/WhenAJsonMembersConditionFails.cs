using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonMembersConditionFails(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X10_10_1)]
    public async Task the_member_is_left_out_of_the_json_body_and_the_endpoint_binds_its_start_not_null()
    {
        var leftOut = await NewWorld();
        await leftOut.Page.GotoAsync(CarePages.MoodCheck);
        await leftOut.Page.GetByLabel("Mood", new PageGetByLabelOptions { Exact = true }).FillAsync("calm");
        await leftOut.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(leftOut.Page.Locator("#mood-echo")).ToHaveTextAsync("not asked");

        var included = await NewWorld();
        await included.Page.GotoAsync(CarePages.MoodCheck);
        await included.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Asked about mood", Exact = true }).CheckAsync();
        await included.Page.GetByLabel("Mood", new PageGetByLabelOptions { Exact = true }).FillAsync("calm");
        await included.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(included.Page.Locator("#mood-echo")).ToHaveTextAsync("calm");
    }
}
