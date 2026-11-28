using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAJsonBodySendsAnEntryKeyedByABool(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_35_0)]
    public async Task the_key_is_named_as_system_text_json_and_mvcs_binding_name_both_write_it_and_binds_its_entry()
    {
        var settled = await NewWorld();
        await settled.Page.GotoAsync(CarePages.BedSettlingOfASettledBed);
        var counted = await settled.Page.RunAndWaitForRequestAsync(() => settled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the bed", Exact = true }).ClickAsync(), "**/bed-settling/counted");
        Assert.That(counted.Method, Is.EqualTo("POST"));
        Assert.That(counted.PostData, Is.EqualTo("{\"settled\":{\"True\":2}}"));
        await Expect(settled.Page.Locator("#counted")).ToHaveTextAsync("Settled 2");

        var unsettled = await NewWorld();
        await unsettled.Page.GotoAsync(CarePages.BedSettlingOfAnUnsettledBed);
        var other = await unsettled.Page.RunAndWaitForRequestAsync(() => unsettled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Count the bed", Exact = true }).ClickAsync(), "**/bed-settling/counted");
        Assert.That(other.PostData, Is.EqualTo("{\"settled\":{\"False\":2}}"));
        await Expect(unsettled.Page.Locator("#counted")).ToHaveTextAsync("Unsettled 2");
    }
}
